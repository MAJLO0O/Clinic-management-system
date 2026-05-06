using Dapper;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace MedicalData.Infrastructure.Repositories
{
    public class BranchRepository
    {
        public async Task<List<int>> branchIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from branch";
            var allIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return allIds.ToList();
        }
        public async Task WriteBranchesJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select id as Id, city as City, address as Address from branch";
            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var cityIndex = reader.GetOrdinal("City");
            var addressIndex = reader.GetOrdinal("Address");

            writer.WriteStartArray();
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();
                var branch = MapToBranch(reader, idIndex, cityIndex, addressIndex);
                JsonSerializer.Serialize(writer, branch);
            }
            writer.WriteEndArray();

            await writer.FlushAsync();
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var path = Path.Combine(Path.GetTempPath(), "exported_branches.json");
            await using var stream = File.Create(path);

            await WriteBranchesJsonAsync(connection, stream, ct);
        }
        public async Task AddBranchesToZipAsync(IDbConnection connection, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_branches.json");
            await using var stream = entry.Open();
            await WriteBranchesJsonAsync(connection, stream, ct);
        }
        public BranchSnapshotDTO MapToBranch(IDataReader reader, int idIndex, int cityIndex, int addressIndex)
        {
            return new BranchSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                City = reader.GetString(cityIndex),
                Address = reader.GetString(addressIndex)
            };
        }


        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_branches.json");
            using var stream = File.OpenRead(path);

            await ImportBranchesAsync(connection, transaction, stream, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_branches.json");
            if (entry == null)
                throw new InvalidOperationException("exported_branches file is null");

            using var stream = entry.Open();
            await ImportBranchesAsync(connection, transaction, stream, cancellationToken);

        }
        public async Task ImportBranchesAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken ct)
        {

            var branches = await JsonSerializer.DeserializeAsync<List<BranchSnapshotDTO>>(stream, cancellationToken: ct);
            if (branches == null)
            {
                throw new Exception("Failed to deserialize branches from JSON. File was empty");
            }
            try
            {
                foreach (var branch in branches)
                {
                    var sql = "insert into branch (id, city, address) values (@Id, @City, @Address);";
                    await connection.ExecuteAsync(sql, branch, transaction: transaction);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('branch','id'),
                    COALESCE((SELECT MAX(id) FROM branch),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql, transaction: transaction);
                Console.WriteLine("Imported branch");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import of table branch failed {ex.Message}");
                throw;
            }
        }
        public async Task<List<BranchSnapshotDTO>> GetBranchesAsync(IDbConnection connection, CancellationToken ct)
        {
            var sql = "select id as Id, city as City, address as Address from branch";
            var branches = await connection.QueryAsync<BranchSnapshotDTO>(new CommandDefinition(sql, cancellationToken: ct));

            return branches.ToList();
        }
    }
}
