using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public  class BranchRepository
    {
        private readonly string _connectionString;
        public BranchRepository(string connectionString) {
            _connectionString = connectionString;
        }
        public async Task<List<int>> branchIds()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select id from branch";
                var allIds = await connection.QueryAsync<int>(sql);
                return allIds.ToList();
            }
        }
    }
}
