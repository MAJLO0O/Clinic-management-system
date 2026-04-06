using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class PersonRepository
    {
        public async Task<HashSet<string>> LoadExistingPesels(IDbConnection connection, IDbTransaction transaction)
        {
                var result = new HashSet<string>();
            var sql = "select pesel from doctor";
                var existingPesels = await connection.QueryAsync<string>(sql,transaction:transaction);
                result.UnionWith(existingPesels);
                sql = "select pesel from patient";
                existingPesels = await connection.QueryAsync<string>(sql,transaction: transaction);
                result.UnionWith(existingPesels);
            return result;
        }
         
        }
        
    }

