using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class PaymentMethodRepository
    {
        private readonly string _connectionString;
        public PaymentMethodRepository(string connectionString) {
        _connectionString = connectionString;
        }
        public async Task<List<int>> GetExistingPaymentMethodIds()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select id from payment_method";
                var paymentMethodIds = await connection.QueryAsync<int>(sql);
                return paymentMethodIds.ToList();
            }
        }
    }
}
