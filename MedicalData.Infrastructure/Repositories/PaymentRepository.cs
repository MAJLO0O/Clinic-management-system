using Dapper;
using MedicalData.Domain.Models;
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
using MedicalData.Infrastructure.ReadDTOs;

namespace MedicalData.Infrastructure.Repositories
{
    public class PaymentRepository
    {
        public async Task<List<int>> GetExistingPaymentIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from payment";
            var paymentIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return paymentIds.ToList();
        }

        public async Task InsertPayments(List<Payment> payments, IDbConnection connection, IDbTransaction transaction)
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
            var paymentNumbers = await connection.QueryAsync<int>(sql, transaction: transaction);
            return paymentNumbers.ToHashSet();
        }
        public async Task WritePaymentsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select id as Id, payment_number as PaymentNumber, amount as Amount, payment_method_id as PaymentMethodId, appointment_id as AppointmentId, payment_status_id as PaymentStatusId from payment";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var paymentNumberIndex = reader.GetOrdinal("PaymentNumber");
            var amountIndex = reader.GetOrdinal("Amount");
            var paymentMethodIdIndex = reader.GetOrdinal("PaymentMethodId");
            var appointmentIdIndex = reader.GetOrdinal("AppointmentId");
            var paymentStatusIdIndex = reader.GetOrdinal("PaymentStatusId");

                writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();

                    var payment = MapToPayment(reader,idIndex,paymentNumberIndex,amountIndex,paymentMethodIdIndex,appointmentIdIndex,paymentStatusIdIndex);
                    JsonSerializer.Serialize(writer, payment);
                }
                writer.WriteEndArray();
             
            await writer.FlushAsync();
           
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_payments.json");
            await using var stream = File.Create(path);

            await WritePaymentsJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection, ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_payments.json");
            await using var entryStream = entry.Open();

            await WritePaymentsJsonAsync(connection, entryStream, ct);
        }
        public PaymentSnapshotDTO MapToPayment(IDataReader reader, int idIndex, int paymentNumberIndex, int amountIndex,
            int paymentMethodIdIndex, int appointmentIdIndex, int paymentStatusIdIndex)
        {
            return new PaymentSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                PaymentNumber = reader.GetInt32(paymentNumberIndex),
                Amount = reader.GetDecimal(amountIndex),
                PaymentMethodId = reader.IsDBNull(paymentMethodIdIndex) ? null : reader.GetInt32(paymentMethodIdIndex),
                AppointmentId = reader.GetInt32(appointmentIdIndex),
                PaymentStatusId = reader.GetInt32(paymentStatusIdIndex)

            };
        }

        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_payments.json");
            using var stream = File.OpenRead(path);

            await ImportPaymentAsync(connection,transaction,stream,cancellationToken);
        }

        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_payments.json");
            if (entry == null)
                throw new FileNotFoundException("The entry exported_payments.json was not found in the zip archive.");
            using var entryStream = entry.Open();
            await ImportPaymentAsync(connection, transaction, entryStream, cancellationToken);
        }
        public async Task ImportPaymentAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken cancellationToken)
        {
            var sql = new StringBuilder();
            sql.Append("insert into payment (id, payment_number, amount, payment_method_id, appointment_id, payment_status_id) values ");
            var parameters = new DynamicParameters();
            var batch = new List<string>();
            try
            {
                await foreach (var payment in JsonSerializer.DeserializeAsyncEnumerable<PaymentSnapshotDTO>(stream,cancellationToken: cancellationToken))
                {
                    int i = batch.Count;
                    if (payment != null)
                    {
                        batch.Add($"(@Id{i}, @PaymentNumber{i}, @Amount{i}, @PaymentMethodId{i}, @AppointmentId{i}, @PaymentStatusId{i})");
                        parameters.Add($"Id{i}", payment.Id);
                        parameters.Add($"PaymentNumber{i}", payment.PaymentNumber);
                        parameters.Add($"Amount{i}", payment.Amount);
                        parameters.Add($"PaymentMethodId{i}", payment.PaymentMethodId);
                        parameters.Add($"AppointmentId{i}", payment.AppointmentId);
                        parameters.Add($"PaymentStatusId{i}", payment.PaymentStatusId);

                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
                            batch.Clear();
                            sql = new StringBuilder();
                            sql.Append("insert into payment (id, payment_number, amount, payment_method_id, appointment_id, payment_status_id) values ");
                            parameters = new DynamicParameters();
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
                }
                        var resetSequenceSql = @"SELECT setval(
                        pg_get_serial_sequence('payment','id'),
                        COALESCE((SELECT MAX(id) FROM payment),1),
                        true
                    );";
                await connection.ExecuteAsync(new CommandDefinition(resetSequenceSql, transaction: transaction, cancellationToken: cancellationToken));

                Console.WriteLine("Imported payment");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed importing payments table {ex.Message} ");
                throw;
            }
        }
        public async Task<PagedResult<ReadPaymentDTO>> GetPaymentsAsync(IDbConnection connection, int lastId, int pageSize, CancellationToken cancellationToken)
        {
            const string sql = @"select p.id as Id, p.payment_number as PaymentNumber,
            p.amount as Amount, pm.method as Method,
            a.starting_date_time as AppointmentStartingDateTime, ps.status as Status,
            coalesce(d.first_name, '') || ' ' || coalesce(d.last_name, '') as DoctorFullName,
            coalesce(pt.first_name, '') || ' ' || coalesce(pt.last_name, '') as PatientFullName
            from payment p
            left join appointment a on p.appointment_id = a.id
            left join patient pt on a.patient_id = pt.id
            left join doctor d on a.doctor_id = d.id
            left join payment_method pm on p.payment_method_id = pm.id
            left join payment_status ps on p.payment_status_id = ps.id
            where p.id > @LastId
            order by p.id asc
            limit @pageSizePlusOne";
            var result = (await connection.QueryAsync<ReadPaymentDTO>(new CommandDefinition(sql, new { LastId = lastId, pageSizePlusOne = pageSize + 1 }, cancellationToken: cancellationToken))).ToList();
            return PaginationHelper.BuildPagedResult(result, pageSize, x => x.Id);
        }
    }
}
