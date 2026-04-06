using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;

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
        public async Task ExportPaymentStatusesAsync(IDbConnection connection)
        {
            var sql = "select id as Id, status as Status from payment_status";
            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_paymentstatuses.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var paymentStatus = new PaymentStatusSnapshotDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        Status = reader.GetString(statusIndex)
                    };
                    JsonSerializer.Serialize(writer, paymentStatus);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting payment statuses: {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportPaymentStatusAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_paymentstatuses.json");
            var json = await File.ReadAllTextAsync(path);
            var statuses = JsonSerializer.Deserialize<List<PaymentStatusSnapshotDTO>>(json);

            try
            {
                foreach (var status in statuses)
                {
                    var sql = "insert into payment_status (id,status) values (@Id,@Status)";

                    await connection.ExecuteAsync(sql, status);
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('payment_status','id'),
                    COALESCE((SELECT MAX(id) FROM payment_status),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
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
