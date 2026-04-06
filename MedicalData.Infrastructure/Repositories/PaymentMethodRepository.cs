using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;

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
        public async Task ExportPaymentMethodsAsync(IDbConnection connection)
        {
            var sql = "select id as Id, method as Method from payment_method";
            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_paymentmethods.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var methodIndex = reader.GetOrdinal("Method");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var paymentMethod = new PaymentMethodSnapshotDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        Method = reader.GetString(methodIndex)
                    };
                    JsonSerializer.Serialize(writer, paymentMethod);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting payment methods: {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportPaymentMethodAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_paymentmethods.json");
            var json = await File.ReadAllTextAsync(path);
            var methods = JsonSerializer.Deserialize<List<PaymentMethodSnapshotDTO>>(json);
            if (methods == null)
                throw new Exception("Couldn't read the method file because it was empty");
            try
            {
                foreach (var method in methods)
                {
                    var sql = "insert into payment_method (id,method) values (@Id,@Method)";
                    await connection.ExecuteAsync(sql, method);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('payment_method','id'),
                    COALESCE((SELECT MAX(id) FROM payment_method),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
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

