using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataGenerator.Models;

namespace DataGenerator.Data
{
    public class PaymentRepository
    {
        private readonly string _connectionString;
        public PaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<int>> GetExistingPaymentIds()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select id from payment";
                var paymentIds = await connection.QueryAsync<int>(sql);
                return paymentIds.ToList();
            }
        }
        public async Task InsertPayments(List<Payment> payments)
        {
            var sql = new StringBuilder();
            sql.Append("insert into payment (payment_number, amount, payment_method_id,appointment_id, payment_status_id) values ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                for (int i = 0; i < payments.Count; i++)
                {
                    values.Add($"(@PaymentNumber{i}, @Amount{i}, @PaymentMethodId{i}, @AppointmentId{i}, @PaymentStatusId{i})");

                    parameters.Add($"PaymentNumber{i}", payments[i].PaymentNumber);
                    parameters.Add($"Amount{i}", payments[i].Amount);
                    parameters.Add($"PaymentMethodId{i}", payments[i].PaymentMethodId);
                    parameters.Add($"AppointmentId{i}", payments[i].AppointmentId);
                    parameters.Add($"PaymentStatusId{i}", payments[i].StatusId);
                }
                sql.Append(string.Join(",", values));
                using var transaction = connection.BeginTransaction();
                try
                {
                    await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;

                }
            }
        }
        public async Task<HashSet<int>> GetExistingPaymentNumbers()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select payment_number from payment";
                var paymentNumbers = await connection.QueryAsync<int>(sql);
                return paymentNumbers.ToHashSet();
            }
    }
    } 
}

