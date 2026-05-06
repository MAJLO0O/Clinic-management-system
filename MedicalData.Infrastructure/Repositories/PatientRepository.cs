using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using MedicalData.Infrastructure.ReadDTOs;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class PatientRepository
    {

        public async Task InsertPatients(List<Patient> patients, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO patient (first_name, last_name,pesel,date_of_birth,phone,email) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            for (int i = 0; i < patients.Count; i++)
            {
                values.Add($"(@FirstName{i}, @LastName{i},@Pesel{i},@DateOfBirth{i},@PhoneNumber{i},@Email{i})");

                parameters.Add($"FirstName{i}", patients[i].FirstName);
                parameters.Add($"LastName{i}", patients[i].LastName);
                parameters.Add($"Pesel{i}", patients[i].Pesel);
                parameters.Add($"DateOfBirth{i}", patients[i].DateOfBirth);
                parameters.Add($"PhoneNumber{i}", patients[i].Phone);
                parameters.Add($"Email{i}", patients[i].Email);
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
        }
        public async Task<List<int>> GetExistingPatientIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from patient";
            var patientIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return patientIds.ToList();
        }

        public async Task<int?> GetPatientByPeselAsync(string pesel, IDbConnection connection)
        {
            var sql = "select id from patient where pesel = @pesel";
            var patientId = await connection.QueryFirstOrDefaultAsync<int>(sql, new { pesel });
            return patientId;
        }
        public async Task WritePatientsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = @"select id as Id, first_name as FirstName, last_name as LastName, pesel as Pesel,
            date_of_birth as DateOfBirth, phone as Phone, email as Email, deleted_at as DeletedAt from patient";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var firstNameIndex = reader.GetOrdinal("FirstName");
            var lastNameIndex = reader.GetOrdinal("LastName");
            var peselIndex = reader.GetOrdinal("Pesel");
            var dateOfBirthIndex = reader.GetOrdinal("DateOfBirth");
            var phoneIndex = reader.GetOrdinal("Phone");
            var emailIndex = reader.GetOrdinal("Email");
            var deletedAtIndex = reader.GetOrdinal("DeletedAt");

            writer.WriteStartArray();
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();

                var patient = MapToPatient(reader, idIndex, firstNameIndex, lastNameIndex, peselIndex, dateOfBirthIndex, phoneIndex, emailIndex, deletedAtIndex);
                JsonSerializer.Serialize(writer, patient);
            }
            writer.WriteEndArray();
            await writer.FlushAsync();
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_patients.json");
            await using var stream = File.Create(path);

            await WritePatientsJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_patients.json");
            await using var entryStream = entry.Open();

            await WritePatientsJsonAsync(connection, entryStream, ct);
        }
        public PatientSnapshotDTO MapToPatient(IDataReader reader, int idIndex, int firstNameIndex, int lastNameIndex, int peselIndex,
            int dateOfBirthIndex, int phoneIndex, int emailIndex, int deletedAtIndex)
        {
            return new PatientSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                FirstName = reader.GetString(firstNameIndex),
                LastName = reader.GetString(lastNameIndex),
                Pesel = reader.GetString(peselIndex),
                DateOfBirth = reader.GetDateTime(dateOfBirthIndex),
                Phone = reader.GetString(phoneIndex),
                Email = reader.GetString(emailIndex),
                DeletedAt = reader.IsDBNull(deletedAtIndex) ? null : reader.GetDateTime(deletedAtIndex)
            };
        }
        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_patients.json");
            using var stream = File.OpenRead(path);
            await ImportPatientsAsync(connection, transaction, stream, cancellationToken);
        }

        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_patients.json");
            if (entry == null)
            {
                throw new FileNotFoundException("exported_patients.json not found in the zip archive.");
            }
            using var entryStream = entry.Open();
            await ImportPatientsAsync(connection, transaction, entryStream, cancellationToken);
        }

        public async Task ImportPatientsAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken cancellationToken)
        {
            var sql = new StringBuilder();
            sql.Append(@"INSERT INTO patient (id, first_name, last_name, pesel, date_of_birth, phone, email, deleted_at) VALUES ");
            var batch = new List<string>();
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var patient in JsonSerializer.DeserializeAsyncEnumerable<PatientSnapshotDTO>(stream, cancellationToken: cancellationToken))
                {
                    if (patient != null)
                    {
                        int i = batch.Count;
                        batch.Add($"(@Id{i},@FirstName{i},@LastName{i},@Pesel{i},@DateOfBirth{i},@Phone{i},@Email{i},@DeletedAt{i})");
                        parameters.Add($"Id{i}", patient.Id);
                        parameters.Add($"FirstName{i}", patient.FirstName);
                        parameters.Add($"LastName{i}", patient.LastName);
                        parameters.Add($"Pesel{i}", patient.Pesel);
                        parameters.Add($"DateOfBirth{i}", patient.DateOfBirth);
                        parameters.Add($"Phone{i}", patient.Phone);
                        parameters.Add($"Email{i}", patient.Email);
                        parameters.Add($"DeletedAt{i}", patient.DeletedAt);
                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
                            batch.Clear();
                            sql = new StringBuilder();
                            sql.Append(@"INSERT INTO patient (id, first_name, last_name, pesel, date_of_birth, phone, email, deleted_at) VALUES ");
                            parameters = new DynamicParameters();
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('patient','id'),
                    COALESCE((SELECT MAX(id) FROM patient),1),
                    true
                    );";
                await connection.ExecuteAsync(new CommandDefinition(resetSequenceSql, transaction: transaction, cancellationToken: cancellationToken));
                Console.WriteLine("Imported patient");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing patients: {ex.Message}");
                throw;
            }
        }

        public async Task<PagedResult<ReadPatientDTO>> GetPatientsAsync(IDbConnection connection, int lastId, int pageSize, CancellationToken ct)
        {
            const string sql = @"select id as Id,
                first_name as FirstName, last_name as LastName,
                pesel as Pesel, date_of_birth as DateOfBirth, phone as Phone,
                email as Email, deleted_at as DeletedAt from patient
                where id > @lastId and deleted_at is null
                 order by id asc
                limit @pageSizePlusOne";
            var result = (await connection.QueryAsync<ReadPatientDTO>(new CommandDefinition(sql, new { lastId, pageSizePlusOne = pageSize + 1 }, cancellationToken: ct))).ToList();

            return PaginationHelper.BuildPagedResult(result, pageSize, x => x.Id);
        }


        public async Task CreatePatientAsync(Patient patient, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = @"insert into patient (first_name, last_name, pesel, date_of_birth, phone, email) 
                        values (@FirstName, @LastName, @Pesel, @DateOfBirth, @Phone, @Email)";
            var parameters = new
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Pesel = patient.Pesel,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email
            };
            await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
        }
        public async Task<bool> UpdatePatientAsync(int id, Patient patient, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = @"update patient set first_name = @FirstName, last_name = @LastName, 
            phone = @Phone, email = @Email where id = @Id and deleted_at is null";
            var parameters = new
            {
                Id = id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Phone = patient.Phone,
                Email = patient.Email
            };
            var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
            return rowsAffected > 0;
        }
        public async Task<bool> DeletePatientAsync(int patientId, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = @"update patient set deleted_at = now() where id = @Id and deleted_at is null";
            var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = patientId }, transaction, cancellationToken: ct));
            return rowsAffected > 0;
        }
    }
}
