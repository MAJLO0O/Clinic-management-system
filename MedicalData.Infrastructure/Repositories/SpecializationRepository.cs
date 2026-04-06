using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.Helpers;
using MedicalData.Infrastructure.DTOs;

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
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_specializations.json");
            await using var stream = File.Create(path);
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
        public async Task ImportSpecializationAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_specializations.json");
            var json = await File.ReadAllTextAsync(path);

            var specializations = JsonSerializer.Deserialize<List<SpecializationSnapshotDTO>>(json);

            if (specializations == null)
            {
                throw new Exception("No specializations found in the JSON file.");
            }
            try
            {
                foreach (var specialization in specializations)
                {
                    var sql = "insert into specialization (id, name) values (@Id, @Name)";

                    await connection.ExecuteAsync(sql, specialization);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('specialization','id'),
                    COALESCE((SELECT MAX(id) FROM specialization),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
                Console.WriteLine("Imported specialization");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing specializations: {ex.Message}");
                throw;

            }
        }
    }
}
