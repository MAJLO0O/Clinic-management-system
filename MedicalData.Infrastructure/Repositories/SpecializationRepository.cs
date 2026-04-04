using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MedicalData.Infrastructure.Repositories
{
    public class SpecializationRepository
    {
        public async Task<List<int>> GetExistingSpecializationIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from specialization";
            var specializationIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return specializationIds.ToList();
        }
        public async Task ExportSpecializationsAsync(IDbConnection connection)
        {
            var sql = "select id as Id, name as Name from specialization";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var stream = File.Create("exported_specializations.json");
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var nameIndex = reader.GetOrdinal("Name");

            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var specialization = new
                    {
                        Id = reader.GetInt32(idIndex),
                        Name = reader.GetString(nameIndex)
                    };
                    JsonSerializer.Serialize(writer, specialization);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting specializations: {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
    }
}
