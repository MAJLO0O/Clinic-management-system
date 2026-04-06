using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace MedicalData.Infrastructure.Repositories
{
    public class MedicalRecordRepository
    {
        public async Task InsertMedicalRecords(List<MedicalRecord> medicalRecords, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            for (int i = 0; i < medicalRecords.Count; i++)
            {
                values.Add($"( @AppointmentId{i}, @Note{i}, @CreatedAt{i})");

                parameters.Add($"@AppointmentId{i}", medicalRecords[i].AppointmentId);
                parameters.Add($"@Note{i}", medicalRecords[i].Note);
                parameters.Add($"@CreatedAt{i}", medicalRecords[i].CreatedAt);


            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
        }
        public async Task ExportMedicalRecordAsync(IDbConnection connection)
        {
            var sql = "select appointment_id as AppointmentId, note as Note, created_at as CreatedAt from medical_record";

            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_medicalrecords.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);

            var appointmentIdIndex = reader.GetOrdinal("AppointmentId");
            var noteIndex = reader.GetOrdinal("Note");
            var createdAtIndex = reader.GetOrdinal("CreatedAt");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var medicalRecord = new MedicalRecordSnapshotDTO
                    {
                        AppointmentId = reader.GetInt32(appointmentIdIndex),
                        Note = reader.GetString(noteIndex),
                        CreatedAt = reader.GetDateTime(createdAtIndex)
                    };
                    JsonSerializer.Serialize(writer, medicalRecord);
                }
                writer.WriteEndArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during export: {ex.Message}");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
        }
        public async Task ImportMedicalRecordsAsync(IDbConnection connection)
        {
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_medicalrecords.json");
            await using var stream = File.OpenRead(path);
            var sql = new StringBuilder();
            sql.Append(@"INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
            using var transaction = connection.BeginTransaction();
            var batch = new List<string>();
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<MedicalRecordSnapshotDTO>(stream))
                {
                    if (record != null)
                    {
                        int i = batch.Count;
                        batch.Add($"(@AppointmentId{i}, @Note{i}, @CreatedAt{i})");
                        parameters.Add($"@AppointmentId{i}", record.AppointmentId);
                        parameters.Add($"@Note{i}", record.Note);
                        parameters.Add($"@CreatedAt{i}", record.CreatedAt);

                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
                            batch.Clear();
                            parameters = new DynamicParameters();
                            sql.Clear();
                            sql.Append(@"INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
                }
                transaction.Commit();
                Console.WriteLine("Imported medical_record");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing medical records: {ex.Message}");
                transaction.Rollback();
                throw;
            }
        }
    }
}