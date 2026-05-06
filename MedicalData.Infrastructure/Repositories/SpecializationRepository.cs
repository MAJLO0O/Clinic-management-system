using Dapper;
using MedicalData.Infrastructure.DTOs;
using System.Data;
using System.IO.Compression;
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
        public async Task WriteSpecializationsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select id as Id, name as Name from specialization";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var nameIndex = reader.GetOrdinal("Name");

            writer.WriteStartArray();
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();

                var specialization = MapToSpecialization(reader, idIndex, nameIndex);
                JsonSerializer.Serialize(writer, specialization);
            }
            writer.WriteEndArray();

            await writer.FlushAsync();

        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_specializations.json");
            await using var stream = File.Create(path);

            await WriteSpecializationsJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_specializations.json");
            await using var entryStream = entry.Open();

            await WriteSpecializationsJsonAsync(connection, entryStream, ct);
        }
        public SpecializationSnapshotDTO MapToSpecialization(IDataReader reader, int idIndex, int nameIndex)
        {
            return new SpecializationSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                Name = reader.GetString(nameIndex)
            };
        }
        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_specializations.json");
            using var stream = File.OpenRead(path);

            await ImportSpecializationAsync(connection, transaction, stream, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.GetEntry("exported_specializations.json");
            if (entry == null)
            {
                throw new FileNotFoundException("The file 'exported_specializations.json' was not found in the ZIP archive.");
            }
            await using var entryStream = entry.Open();
            await ImportSpecializationAsync(connection, transaction, entryStream, ct);
        }

        public async Task ImportSpecializationAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken ct)
        {

            var specializations = await JsonSerializer.DeserializeAsync<List<SpecializationSnapshotDTO>>(stream, cancellationToken: ct);

            if (specializations == null || !specializations.Any())
            {
                throw new InvalidOperationException("Specializations file is empty or invalid.");
            }
            try
            {
                foreach (var specialization in specializations)
                {
                    var sql = "insert into specialization (id, name) values (@Id, @Name)";

                    await connection.ExecuteAsync(new CommandDefinition(sql, specialization, transaction: transaction, cancellationToken: ct));
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('specialization','id'),
                    COALESCE((SELECT MAX(id) FROM specialization),1),
                    true
                    );";
                await connection.ExecuteAsync(new CommandDefinition(resetSequenceSql, transaction: transaction, cancellationToken: ct));
                Console.WriteLine("Imported specialization");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing specializations: {ex.Message}");
                throw;

            }
        }



        public async Task<List<SpecializationSnapshotDTO>> GetSpecializationsAsync(IDbConnection connection, CancellationToken ct)
        {
            var sql = "select id as Id, name as Name from specialization";
            var specializations = await connection.QueryAsync<SpecializationSnapshotDTO>(new CommandDefinition(sql,cancellationToken: ct));
            return specializations.ToList();
        }
    }
}
