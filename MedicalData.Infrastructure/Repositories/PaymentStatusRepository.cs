using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;

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
            await using var stream = File.Create("exported_paymentstatuses.json");
            using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var paymentStatus = new
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
    }
}
