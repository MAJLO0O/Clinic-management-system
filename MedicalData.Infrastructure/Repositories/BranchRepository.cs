using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;

namespace MedicalData.Infrastructure.Repositories
{
    public  class BranchRepository
    {
        public async Task<List<int>> branchIds(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select id from branch";
                var allIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                return allIds.ToList();
        }
        public async Task ExportBranchesAsync(IDbConnection connection)
        {
            var sql = "select id as Id, city as City, address as Address from branch";
            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(),"exported_branches.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);
            
            var idIndex = reader.GetOrdinal("Id");
            var cityIndex = reader.GetOrdinal("City");
            var addressIndex = reader.GetOrdinal("Address");
            
            try
            {
                writer.WriteStartArray();
                while(reader.Read())
                {
                    var branch = new BranchSnapshotDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        City = reader.GetString(cityIndex),
                        Address = reader.GetString(addressIndex)
                    };
                    JsonSerializer.Serialize(writer, branch);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export of table branch failed {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportBranchesAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_branches.json");
            var stream = await File.ReadAllTextAsync(path);

            var branches = JsonSerializer.Deserialize<List<BranchSnapshotDTO>>(stream);
            if (branches == null)
            {
                throw new Exception("Failed to deserialize branches from JSON. File was empty");
            }
            try
            {
                foreach (var branch in branches)
                {
                    var sql = "insert into branch (id, city, address) values (@Id, @City, @Address);";
                    await connection.ExecuteAsync(sql, branch);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('branch','id'),
                    COALESCE((SELECT MAX(id) FROM branch),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
                Console.WriteLine("Imported branch");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import of table branch failed {ex.Message}");
                throw;
            }
        }

    }

}
