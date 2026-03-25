using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class PaymentMethodRepository
    {
        public async Task<List<int>> GetExistingPaymentMethodIds(IDbConnection connection,IDbTransaction transaction)
         {
                var sql = "select id from payment_method";
                var paymentMethodIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                return paymentMethodIds.ToList();
        }
    }
}
