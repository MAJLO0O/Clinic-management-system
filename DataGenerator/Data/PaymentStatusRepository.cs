using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DataGenerator.Data
{
    public class PaymentStatusRepository
    {
        private readonly string _connectionString;
            public PaymentStatusRepository(string connectionString) {
            _connectionString = connectionString;
            }
            public async Task<List<int>> GetExistingPaymentStatusIds()
            {
                using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select id from payment_status";
                    var paymentStatusIds = await connection.QueryAsync<int>(sql);
                    return paymentStatusIds.ToList();
                }
        }
    }
}
