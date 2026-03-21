using Dapper;
using DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class PersonRepository
    {
        private readonly string _connectionString;
        public PersonRepository(string connectionString)
        {
            _connectionString = connectionString;
        }



        public async Task LoadExistingPesels()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select pesel from doctor";
                var existingPesels = await connection.QueryAsync<string>(sql);
                GeneratorMethods.GeneratedPesels.UnionWith(existingPesels);
                sql = "select pesel from patient";
                existingPesels = await connection.QueryAsync<string>(sql);
                GeneratorMethods.GeneratedPesels.UnionWith(existingPesels);
            }
        }
        
    }
}
