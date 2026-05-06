using Dapper;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using Microsoft.VisualBasic;
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
    public class PaymentMethodRepository
    {
        public async Task<List<int>> GetExistingPaymentMethodIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from payment_method";
            var paymentMethodIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return paymentMethodIds.ToList();
        }
        public async Task WritePaymentMethodsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select id as Id, method as Method from payment_method";
            using var reader = await connection.ExecuteReaderAsync(sql);

            await using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var methodIndex = reader.GetOrdinal("Method");
             
            writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();
                    var paymentMethod = MapToPaymentMethod(reader, idIndex, methodIndex);
                    JsonSerializer.Serialize(writer, paymentMethod);
                }
                writer.WriteEndArray();
             
            await writer.FlushAsync();
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_payment_methods.json");
            await using var stream = File.Create(path);
            
            await WritePaymentMethodsJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection, ZipArchive zip,CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_payment_methods.json");
            await using var entryStream = entry.Open();

            await WritePaymentMethodsJsonAsync(connection, entryStream, ct);
        }
        public PaymentMethodSnapshotDTO MapToPaymentMethod(IDataReader reader, int idIndex, int methodIndex)
        {
            return new PaymentMethodSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                Method = reader.GetString(methodIndex)
            };
        }


        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_payment_methods.json");
            using var stream = File.OpenRead(path);
            await ImportPaymentMethodAsync(connection, transaction, stream, cancellationToken);
        }

        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_payment_methods.json");
            if (entry == null)
            {
                throw new FileNotFoundException("exported_payment_methods.json not found in the zip archive.");
            }
            using var entryStream = entry.Open();
            await ImportPaymentMethodAsync(connection, transaction, entryStream, cancellationToken);
        }

        public async Task ImportPaymentMethodAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken cancellationToken)
        {
            var json = await JsonSerializer.DeserializeAsync<List<PaymentMethodSnapshotDTO>>(stream, cancellationToken: cancellationToken);
            if (json == null)
                throw new InvalidOperationException("Couldn't read the method file because it was empty");
            var sql = "insert into payment_method (id,method) values (@Id,@Method)";
            try
            {
                foreach (var method in json)
                {
                    await connection.ExecuteAsync(new CommandDefinition(sql, method, transaction, cancellationToken: cancellationToken));
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('payment_method','id'),
                    COALESCE((SELECT MAX(id) FROM payment_method),1),
                    true
                    );";
                await connection.ExecuteAsync(new CommandDefinition(resetSequenceSql, transaction, cancellationToken: cancellationToken));
                Console.WriteLine("Imported payment_method");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing payment methods: {ex.Message}");
                throw;
            }
        }
    }
}

