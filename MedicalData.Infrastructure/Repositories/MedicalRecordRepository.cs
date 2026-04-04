using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
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
            await using var stream = File.Create("exported_medicalrecords.json");
            using var writer = new Utf8JsonWriter(stream);

            var appointmentIdIndex = reader.GetOrdinal("AppointmentId");
            var noteIndex = reader.GetOrdinal("Note");
            var createdAtIndex = reader.GetOrdinal("CreatedAt");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var medicalRecord = new ExportMedicalRecordDTO
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
    }
}