using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
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
                parameters.Add($"PhoneNumber{i}", patients[i].PhoneNumber);
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
            var sql = "select id from patient where pesel = @pesel}";
            var patientId = await connection.QueryFirstOrDefaultAsync<int>(sql, new { pesel });
            return patientId;
        }
        public async Task ExportPatientsAsync(IDbConnection connection)
        {
            var sql = @"select id as Id, first_name as FirstName, last_name as LastName, pesel as Pesel,
            date_of_birth as DateOfBirth, phone as Phone, email as Email, deleted_at as DeletedAt from patient";

            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_patients.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var firstNameIndex = reader.GetOrdinal("FirstName");
            var lastNameIndex = reader.GetOrdinal("LastName");
            var peselIndex = reader.GetOrdinal("Pesel");
            var dateOfBirthIndex = reader.GetOrdinal("DateOfBirth");
            var phoneIndex = reader.GetOrdinal("Phone");
            var emailIndex = reader.GetOrdinal("Email");
            var deletedAtIndex = reader.GetOrdinal("DeletedAt");

            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var patient = new PatientSnapshotDTO
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
                    JsonSerializer.Serialize(writer, patient);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting patients: {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportPatientsAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_patients.json");
            await using var stream = File.OpenRead(path);
            var sql = new StringBuilder();
            sql.Append(@"INSERT INTO patient (id, first_name, last_name, pesel, date_of_birth, phone, email, deleted_at) VALUES ");
            using var transaction = connection.BeginTransaction();
            var batch = new List<string>();
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var patient in JsonSerializer.DeserializeAsyncEnumerable<PatientSnapshotDTO>(stream))
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
                            await connection.ExecuteAsync(sql.ToString(), parameters,transaction);
                            batch.Clear();
                            sql = new StringBuilder();
                            sql.Append(@"INSERT INTO patient (id, first_name, last_name, pesel, date_of_birth, phone, email, deleted_at) VALUES ");
                            batch = new List<string>();
                            parameters = new DynamicParameters();
                        }
                    }
                }
                if(batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(sql.ToString(), parameters,transaction);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('patient','id'),
                    COALESCE((SELECT MAX(id) FROM patient),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
                transaction.Commit();
                Console.WriteLine("Imported patient");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing patients: {ex.Message}");
                transaction.Rollback();
                throw;
            }
        }
    }
}
