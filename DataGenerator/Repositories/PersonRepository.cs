using Dapper;
using DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class PersonRepository
    {
        public async Task LoadExistingPesels(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select pesel from doctor";
                var existingPesels = await connection.QueryAsync<string>(sql,transaction:transaction);
                GeneratorMethods.GeneratedPesels.UnionWith(existingPesels);
                sql = "select pesel from patient";
                existingPesels = await connection.QueryAsync<string>(sql,transaction: transaction);
                GeneratorMethods.GeneratedPesels.UnionWith(existingPesels);
            }
        }
        
    }

