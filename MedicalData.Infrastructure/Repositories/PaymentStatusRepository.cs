using Dapper;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class PaymentStatusRepository
    {
            public async Task<List<int>> GetExistingPaymentStatusIds(IDbConnection connection, IDbTransaction transaction)
            {
                    var sql = "select id from payment_status";
                    var paymentStatusIds = await connection.QueryAsync<int>(sql,transaction: transaction);
                    return paymentStatusIds.ToList();
            }
        public async Task WritePaymentStatusesJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select id as Id, status as Status from payment_status";
            using var reader = await connection.ExecuteReaderAsync(sql);

            await using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");

                writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();

                    var paymentStatus = MapToPaymentStatus(reader,idIndex,statusIndex);
                    JsonSerializer.Serialize(writer, paymentStatus);
                }
                writer.WriteEndArray();
            
                await writer.FlushAsync();
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_payment_statuses.json");
            await using var stream = File.Create(path);

            await WritePaymentStatusesJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection,ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_payment_statuses.json");
            await using var entryStream = entry.Open();

            await WritePaymentStatusesJsonAsync(connection, entryStream, ct);  
        }
        public PaymentStatusSnapshotDTO MapToPaymentStatus(IDataReader reader, int idIndex, int statusIndex)
        {
            return new PaymentStatusSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                Status = reader.GetString(statusIndex)
            };
        }
        public async Task ImportFromJsonAsync(IDbConnection connection,IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_payment_statuses.json");
            using var stream = File.OpenRead(path);

            await ImportPaymentStatusAsync(connection, stream, transaction, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction,ZipArchive zip , CancellationToken ct)
        {
            var entry = zip.GetEntry("exported_payment_statuses.json");
            if (entry == null)
            {
                throw new FileNotFoundException("exported_payment_statuses.json not found in the zip archive.");
            }
            await using var entryStream = entry.Open();
            await ImportPaymentStatusAsync(connection, entryStream, transaction, ct);
        }


        public async Task ImportPaymentStatusAsync(IDbConnection connection, Stream stream, IDbTransaction transaction, CancellationToken ct)
        {
            var statuses = await JsonSerializer.DeserializeAsync<List<PaymentStatusSnapshotDTO>>(stream, cancellationToken: ct);
            if(statuses == null || !statuses.Any())
            {
                throw new InvalidOperationException("Payment status file is empty or invalid.");
            }
            try
            {
                foreach (var status in statuses)
                {
                    var sql = "insert into payment_status (id,status) values (@Id,@Status)";

                    await connection.ExecuteAsync(new CommandDefinition(sql, status,transaction: transaction, cancellationToken: ct));
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('payment_status','id'),
                    COALESCE((SELECT MAX(id) FROM payment_status),1),
                    true
                    );";
                await connection.ExecuteAsync(new CommandDefinition(resetSequenceSql, transaction: transaction, cancellationToken: ct));
                Console.WriteLine("Imported payment_status");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing payment status: {ex.Message}");
                throw;
            }
        }
    }
}
