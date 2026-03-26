using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DataGenerator.Services
{
    public class DataCleanerService
    {
        private readonly string _connectionString;
        public DataCleanerService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task ClearAllAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync("TRUNCATE TABLE payment, medical_record, appointment, doctor_specialization, doctor, patient RESTART IDENTITY CASCADE;", transaction: transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
