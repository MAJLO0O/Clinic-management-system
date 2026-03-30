using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace MedicalData.Infrastructure.Repositories
{
    public class PaymentStatusRepository
    {
            public async Task<List<int>> GetExistingPaymentStatusIds(IDbConnection connection, IDbTransaction transaction)
            {
                    var sql = "select id from payment_status";
                    var paymentStatusIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                    return paymentStatusIds.ToList();
                }
        }
}
