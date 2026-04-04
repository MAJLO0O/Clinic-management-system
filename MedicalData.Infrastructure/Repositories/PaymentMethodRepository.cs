using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;

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
            await using var stream = File.Create("exported_paymentmethods.json");
            using var writer = new Utf8JsonWriter(stream);
            var idIndex = reader.GetOrdinal("Id");
            var methodIndex = reader.GetOrdinal("Method");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var paymentMethod = new ExportPaymentMethodDTO
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
    }
}

