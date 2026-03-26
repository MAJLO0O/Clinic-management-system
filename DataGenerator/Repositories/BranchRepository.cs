using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DataGenerator.Data
{
    public  class BranchRepository
    {
        public async Task<List<int>> branchIds(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select id from branch";
                var allIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                return allIds.ToList();
        }
    }
}
