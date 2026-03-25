using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class SpecializationRepository
    {
        public async Task<List<int>> GetExistingSpecializationIds(IDbConnection connection, IDbTransaction transaction)
            {
                    var sql = "select id from specialization";
                    var specializationIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                    return specializationIds.ToList();
        }
    }
}
