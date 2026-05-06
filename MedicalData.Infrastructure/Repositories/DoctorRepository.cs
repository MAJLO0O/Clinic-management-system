using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.ReadDTOs;
using MedicalData.Infrastructure.Helpers;
using Npgsql;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;


namespace MedicalData.Infrastructure.Repositories
{
    public class DoctorRepository
    {
        public async Task<int> CountDoctorsAsync(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select count(*) from doctor";
            var result = await connection.ExecuteScalarAsync<int>(sql, transaction: transaction);
            if (result == 0) return 1;
            return result;

        }
        public async Task InsertDoctors(List<Doctor> doctors, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor (first_name, last_name,pesel,phone_number,email,branch_id) VALUES ");
            var parameters = new DynamicParameters();
            List<string> values = new();

            for (int i = 0; i < doctors.Count; i++)
            {
                values.Add($"(@FirstName{i},@LastName{i},@Pesel{i},@PhoneNumber{i},@Email{i},@BranchId{i})");

                parameters.Add($"FirstName{i}", doctors[i].FirstName);
                parameters.Add($"LastName{i}", doctors[i].LastName);
                parameters.Add($"Pesel{i}", doctors[i].Pesel);
                parameters.Add($"PhoneNumber{i}", doctors[i].Phone);
                parameters.Add($"Email{i}", doctors[i].Email);
                parameters.Add($"BranchId{i}", doctors[i].BranchId);
            }

            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters, transaction: transaction);

        }



        public async Task<List<int>> GetExistingDoctorsIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from doctor";
            var doctorsIds = await connection.QueryAsync<int>(sql, transaction);
            return doctorsIds.ToList();
        }
        public async Task<List<int>> GetExistingDoctorIdsWithoutSpecialization(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select d.id from doctor d left join doctor_specialization ds on d.id = ds.doctor_id where ds.specialization_id is null";
            var doctorIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return doctorIds.ToList();
        }
        public async Task<int?> GetDoctorByPeselAsync(string pesel, IDbConnection connection)
        {
            var sql = "select id from doctor where pesel = @pesel}";
            var DoctorId = await connection.QueryFirstOrDefaultAsync<int>(sql, new { pesel });
            return DoctorId;
        }

        public async Task WriteDoctorsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = @"select id as Id, first_name as FirstName, last_name as LastName, pesel as Pesel,
                        phone_number as PhoneNumber, email as Email,
                        deleted_at as DeletedAt, created_at as CreatedAt,branch_id as BranchId from doctor";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var firstNameIndex = reader.GetOrdinal("FirstName");
            var lastNameIndex = reader.GetOrdinal("LastName");
            var peselIndex = reader.GetOrdinal("Pesel");
            var phoneNumberIndex = reader.GetOrdinal("PhoneNumber");
            var emailIndex = reader.GetOrdinal("Email");
            var deletedAtIndex = reader.GetOrdinal("DeletedAt");
            var createdAtIndex = reader.GetOrdinal("CreatedAt");
            var branchIdIndex = reader.GetOrdinal("BranchId");

            writer.WriteStartArray();
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                var doctor = MapToDoctor(reader, idIndex, firstNameIndex, lastNameIndex, peselIndex, phoneNumberIndex, emailIndex, deletedAtIndex, createdAtIndex, branchIdIndex);
                JsonSerializer.Serialize(writer, doctor);
            }
            writer.WriteEndArray();

            await writer.FlushAsync();
        }

        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var path = Path.Combine(Path.GetTempPath(), "exported_doctors.json");

            await using var stream = File.Create(path);

            await WriteDoctorsJsonAsync(connection, stream, ct);
        }
        public async Task AddDoctorsToZipAsync(IDbConnection connection, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_doctors.json");

            await using var entryStream = entry.Open();
            await WriteDoctorsJsonAsync(connection, entryStream, ct);
        }

        public DoctorSnapshotDTO MapToDoctor(IDataReader reader, int idIndex, int firstNameIndex, int lastNameIndex, int peselIndex, int phoneNumberIndex, int emailIndex,
            int deletedAtIndex, int createdAtIndex, int branchIdIndex)
        {
            return new DoctorSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                FirstName = reader.GetString(firstNameIndex),
                LastName = reader.GetString(lastNameIndex),
                Pesel = reader.GetString(peselIndex),
                PhoneNumber = reader.GetString(phoneNumberIndex),
                Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                DeletedAt = reader.IsDBNull(deletedAtIndex) ? null : reader.GetDateTime(deletedAtIndex),
                CreatedAt = reader.GetDateTime(createdAtIndex),
                BranchId = reader.GetInt32(branchIdIndex)
            };
        }





        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_doctors.json");
            using var stream = File.OpenRead(path);

            await ImportDoctorsAsync(connection, transaction, stream, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_doctors.json");
            if (entry == null)
                throw new InvalidOperationException("exported_doctors file is null");

            using var stream = entry.Open();
            await ImportDoctorsAsync(connection, transaction, stream, cancellationToken);

        }

        public async Task ImportDoctorsAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken ct)
        {
            var batch = new List<string>();
            var sql = new StringBuilder();
            sql.Append(@"insert into doctor (id,first_name,last_name,pesel,phone_number,email,deleted_at,created_at,branch_id) values ");
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var doctor in JsonSerializer.DeserializeAsyncEnumerable<DoctorSnapshotDTO>(stream, cancellationToken: ct))
                {
                    if (doctor != null)
                    {
                        int i = batch.Count;
                        batch.Add($"(@Id{i}, @FirstName{i}, @LastName{i}, @Pesel{i},@PhoneNumber{i}, @Email{i}, @DeletedAt{i}, @CreatedAt{i}, @BranchId{i} )");
                        parameters.Add($"Id{i}", doctor.Id);
                        parameters.Add($"FirstName{i}", doctor.FirstName);
                        parameters.Add($"LastName{i}", doctor.LastName);
                        parameters.Add($"Pesel{i}", doctor.Pesel);
                        parameters.Add($"PhoneNumber{i}", doctor.PhoneNumber);
                        parameters.Add($"Email{i}", doctor.Email);
                        parameters.Add($"DeletedAt{i}", doctor.DeletedAt);
                        parameters.Add($"CreatedAt{i}", doctor.CreatedAt);
                        parameters.Add($"BranchId{i}", doctor.BranchId);

                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: ct));
                            batch.Clear();
                            sql = new StringBuilder();
                            sql.Append("insert into doctor (id,first_name,last_name,pesel,phone_number,email,deleted_at,created_at,branch_id) values ");
                            parameters = new DynamicParameters();
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: ct));
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('doctor','id'),
                    COALESCE((SELECT MAX(id) FROM doctor),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql, transaction);
                Console.WriteLine("Imported Doctors");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing doctors: {ex.Message}");
                throw;
            }
        }
        public async Task<PagedResult<ReadDoctorDTO>> GetDoctorsAsync(IDbConnection connection, int lastId, int pageSize, CancellationToken ct)
        {
            const string sql = @"SELECT d.id as Id, d.first_name as FirstName, d.last_name as LastName, d.pesel as Pesel,
                        d.phone_number as Phone, d.email as Email,
                        d.deleted_at as DeletedAt, d.created_at as CreatedAt, b.city as City,
                        COALESCE (array_agg(distinct s.name) filter (where s.name is not null), '{}') as Specializations
                        FROM doctor d
                        JOIN branch b ON d.branch_id = b.id
                        LEFT JOIN doctor_specialization ds ON d.id = ds.doctor_id
                        LEFT JOIN specialization s ON ds.specialization_id = s.id
                        where d.id > @lastId and d.deleted_at is null
                        GROUP BY d.id, b.city
                        ORDER BY d.id ASC
                        limit @pageSizePlusOne";
            var result = (await connection.QueryAsync<ReadDoctorDTO>(new CommandDefinition(sql, new { lastId, pageSizePlusOne = pageSize + 1 }, cancellationToken: ct))).ToList();
            return PaginationHelper.BuildPagedResult(result, pageSize, x => x.Id);
        }



        public async Task<int> InsertDoctorAsync(IDbConnection connection, IDbTransaction transaction,Doctor doctor, CancellationToken ct)
        {
            var sql = @"INSERT INTO doctor (first_name, last_name,pesel,phone_number,email,branch_id) VALUES 
                        (@FirstName,@LastName,@Pesel,@PhoneNumber,@Email,@BranchId) returning id";
            var parameters = new
            {
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Pesel = doctor.Pesel,
                PhoneNumber = doctor.Phone,
                Email = doctor.Email,
                BranchId = doctor.BranchId
            };
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
        }
        public async Task<bool> UpdateDoctorAsync(int Id, Doctor doctor, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = @"UPDATE doctor SET first_name = @FirstName, last_name = @LastName,
                        phone_number = @PhoneNumber, email = @Email, branch_id = @BranchId
                        WHERE id = @Id and deleted_at is null";
            var parameters = new
            {
                Id = Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                PhoneNumber = doctor.Phone,
                Email = doctor.Email,
                BranchId = doctor.BranchId
            };
            var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
            return rowsAffected > 0;
        }
        public async Task<bool> SoftDeleteDoctorAsync(IDbConnection connection, IDbTransaction transaction, int Id, CancellationToken ct)
        {
            var sql = @"UPDATE doctor SET deleted_at = Now() WHERE id = @Id and deleted_at is null";
            var parameters = new
            {
                Id = Id,
            };
            var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
            return rowsAffected > 0;
        }

        public async Task InsertNewRelationsAsync(int doctorId, List<int> specializationIds, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
            var parameters = new DynamicParameters();
            List<string> values = new();
            for (int i = 0; i < specializationIds.Count; i++)
            {
                values.Add($"(@DoctorId{i}, @SpecializationId{i})");
                parameters.Add($"DoctorId{i}", doctorId);
                parameters.Add($"SpecializationId{i}", specializationIds[i]);
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: ct));
        }
        public async Task RemoveRelationsAsync(int doctorId, IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var sql = @"DELETE FROM doctor_specialization WHERE doctor_id = @DoctorId";
            await connection.ExecuteAsync(new CommandDefinition(sql, new { DoctorId = doctorId }, transaction, cancellationToken: ct));
        }
    }
}
