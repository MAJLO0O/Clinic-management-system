using Dapper;
using DataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DataGenerator.Data
{
    public class MedicalRecordRepository
    {
        private readonly string _connectionString;
        public MedicalRecordRepository(string connectionString) 
        {
            _connectionString = connectionString;
        }
        public async Task InsertMedicalRecords(List<MedicalRecord> medicalRecords)
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {   
                var sql = new StringBuilder();
                sql.Append("INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
                var parameters = new DynamicParameters();
                var values = new List<string>();
                await connection.OpenAsync();
                for(int i=0; i < medicalRecords.Count; i++)
                {
                    values.Add($"( @AppointmentId{i}, @Note{i}, @CreatedAt{i})");
                    
                    parameters.Add($"@AppointmentId{i}", medicalRecords[i].AppointmentId);
                    parameters.Add($"@Note{i}", medicalRecords[i].Note);
                    parameters.Add($"@CreatedAt{i}", medicalRecords[i].CreatedAt);

                    
                }
                sql.Append(string.Join(",", values));
                using var transaction = connection.BeginTransaction();
                try
                {
                    
                    await connection.ExecuteAsync(sql.ToString(), parameters,transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting medical records: {ex.Message}");
                    transaction.Rollback();
                }

            }
        }
    }
}
