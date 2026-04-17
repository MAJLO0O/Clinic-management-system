using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;
using System.Threading.Tasks;
using System.Transactions;
using MedicalData.Infrastructure.Helpers;


namespace MedicalData.Infrastructure.Repositories
{
    public class DoctorRepository
    {
        public async Task<int> CountDoctorsAsync(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select count(*) from doctor";
            return await connection.ExecuteScalarAsync<int>(sql, transaction: transaction);

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
                parameters.Add($"PhoneNumber{i}", doctors[i].PhoneNumber);
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

        public async Task ExportDoctorsAsync(IDbConnection connection)
        {
            var sql = @"select id as Id, first_name as FirstName, last_name as LastName, pesel as Pesel,
                        phone_number as PhoneNumber, email as Email,
                        deleted_at as DeletedAt, created_at as CreatedAt,branch_id as BranchId from doctor";

            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_doctors.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var firstNameIndex = reader.GetOrdinal("FirstName");
            var lastNameIndex = reader.GetOrdinal("LastName");
            var peselIndex = reader.GetOrdinal("Pesel");
            var phoneNumberIndex = reader.GetOrdinal("PhoneNumber");
            var emailIndex = reader.GetOrdinal("Email");
            var deletedAtIndex = reader.GetOrdinal("DeletedAt");
            var createdAtIndex = reader.GetOrdinal("CreatedAt");
            var branchIdIndex = reader.GetOrdinal("BranchId");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var doctor = new DoctorSnapshotDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        FirstName = reader.GetString(firstNameIndex),
                        LastName = reader.GetString(lastNameIndex),
                        Pesel = reader.GetString(peselIndex),
                        PhoneNumber = reader.GetString(phoneNumberIndex),
                        Email = reader.GetString(emailIndex),
                        DeletedAt = reader.IsDBNull(deletedAtIndex) ? null : reader.GetDateTime(deletedAtIndex),
                        CreatedAt = reader.GetDateTime(createdAtIndex),
                        BranchId = reader.GetInt32(branchIdIndex)
                    };
                    JsonSerializer.Serialize(writer, doctor);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export of table doctor failed {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportDoctorsAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_doctors.json");
            await using var json = File.OpenRead(path);
            var batch = new List<string>();
            var sql = new StringBuilder();
            sql.Append(@"insert into doctor (id,first_name,last_name,pesel,phone_number,email,deleted_at,created_at,branch_id) values ");
            var parameters = new DynamicParameters();
            using var transaction = connection.BeginTransaction();
            try
            {
                await foreach (var doctor in JsonSerializer.DeserializeAsyncEnumerable<DoctorSnapshotDTO>(json))
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
                            await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
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
                        await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
                    }
                    var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('doctor','id'),
                    COALESCE((SELECT MAX(id) FROM doctor),1),
                    true
                    );";
                    await connection.ExecuteAsync(resetSequenceSql);
                    transaction.Commit();
                    Console.WriteLine("Imported Doctors");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing doctors: {ex.Message}");
                transaction.Rollback();
                throw;
            }
        }
    }
}
