using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class SpecializationRepository
    {
        private readonly string _connectionString;
        public SpecializationRepository(string connectionString) 
        {
            _connectionString = connectionString;
        }
        public async Task<List<int>> GetExistingSpecializationIds(string connectionString)
            {
                using (var connection = DbConnectionFactory.CreateDbConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select id from specialization";
                    var specializationIds = await connection.QueryAsync<int>(sql);
                    return specializationIds.ToList();
                }
        }
    }
}
