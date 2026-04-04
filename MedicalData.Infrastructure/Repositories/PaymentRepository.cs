using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;

namespace DataGenerator.Data
{
    public class PaymentRepository
    {
        public async Task<List<int>> GetExistingPaymentIds(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select id from payment";
                var paymentIds = await connection.QueryAsync<int>(sql, transaction: transaction);
                return paymentIds.ToList();
            }
       
        public async Task InsertPayments(List<Payment> payments,IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("insert into payment (payment_number, amount, payment_method_id,appointment_id, payment_status_id) values ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
                for (int i = 0; i < payments.Count; i++)
                {
                    values.Add($"(@PaymentNumber{i}, @Amount{i}, @PaymentMethodId{i}, @AppointmentId{i}, @PaymentStatusId{i})");

                    parameters.Add($"PaymentNumber{i}", payments[i].PaymentNumber);
                    parameters.Add($"Amount{i}", payments[i].Amount);
                    parameters.Add($"PaymentMethodId{i}", payments[i].PaymentMethodId);
                    parameters.Add($"AppointmentId{i}", payments[i].AppointmentId);
                    parameters.Add($"PaymentStatusId{i}", payments[i].StatusId);
                }
                sql.Append(string.Join(",", values));
                    await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
        }
        public async Task<HashSet<int>> GetExistingPaymentNumbers(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select payment_number from payment";
                var paymentNumbers = await connection.QueryAsync<int>(sql,transaction: transaction);
                return paymentNumbers.ToHashSet();
            }
        public async Task ExportPaymentsAsync(IDbConnection connection)
        {
            var sql = "select id as Id, payment_number as PaymentNumber, amount as Amount, payment_method_id as PaymentMethodId, appointment_id as AppointmentId, payment_status_id as PaymentStatusId from payment";
            
            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var stream = File.Create("exported_payments.json");
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var paymentNumberIndex = reader.GetOrdinal("PaymentNumber");
            var amountIndex = reader.GetOrdinal("Amount");
            var paymentMethodIdIndex = reader.GetOrdinal("PaymentMethodId");
            var appointmentIdIndex = reader.GetOrdinal("AppointmentId");
            var paymentStatusIdIndex = reader.GetOrdinal("PaymentStatusId");

            try
            {
                writer.WriteStartArray();
                while(reader.Read())
                {
                    var payment = new ExportPaymentDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        PaymentNumber = reader.GetInt32(paymentNumberIndex),
                        Amount = reader.GetDecimal(amountIndex),
                        PaymentMethodId = reader.IsDBNull(paymentMethodIdIndex) ? null : reader.GetInt32(paymentMethodIdIndex),
                        AppointmentId = reader.GetInt32(appointmentIdIndex),
                        PaymentStatusId = reader.GetInt32(paymentStatusIdIndex)

                    };
                    JsonSerializer.Serialize(writer, payment);
                }
                writer.WriteEndArray();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed exporting payments table {ex.Message} ");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
    }
    } 
