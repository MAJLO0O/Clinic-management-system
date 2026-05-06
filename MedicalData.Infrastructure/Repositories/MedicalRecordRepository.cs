using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
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
        public async Task WriteMedicalRecordsJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select appointment_id as AppointmentId, note as Note, created_at as CreatedAt from medical_record";

            using var reader = await connection.ExecuteReaderAsync(sql);
            using var writer = new Utf8JsonWriter(stream);

            var appointmentIdIndex = reader.GetOrdinal("AppointmentId");
            var noteIndex = reader.GetOrdinal("Note");
            var createdAtIndex = reader.GetOrdinal("CreatedAt");
                writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();
                    var medicalRecord = MapToMedicalRecord(reader,appointmentIdIndex,noteIndex,createdAtIndex);
                    JsonSerializer.Serialize(writer, medicalRecord);
                }
                writer.WriteEndArray();

                await writer.FlushAsync();
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var path = Path.Combine(Path.GetTempPath(), "exported_medical_records.json");
            await using var stream = File.Create(path);

            await WriteMedicalRecordsJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection,ZipArchive zip ,CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_medical_records.json");
            await using var entryStream = entry.Open();

            await WriteMedicalRecordsJsonAsync(connection, entryStream, ct);
        }

        public MedicalRecordSnapshotDTO MapToMedicalRecord(IDataReader reader, int appointmentIdIndex, int noteIndex, int createdAtIndex)
        {
            return new MedicalRecordSnapshotDTO
            {
                AppointmentId = reader.GetInt32(appointmentIdIndex),
                Note = reader.GetString(noteIndex),
                CreatedAt = reader.GetDateTime(createdAtIndex)
            };
        }

        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_medical_records.json");
            using var stream = File.OpenRead(path);

            await ImportMedicalRecordsAsync(connection, transaction, stream, ct);
        }

        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_medical_records.json");
            if (entry == null)
            {
                throw new FileNotFoundException("The file exported_medical_records.json was not found in the zip archive.");
            }
            using var entryStream = entry.Open();
            await ImportMedicalRecordsAsync(connection, transaction, entryStream, cancellationToken);
        }


        public async Task ImportMedicalRecordsAsync(IDbConnection connection,IDbTransaction transaction, Stream stream, CancellationToken cancellationToken)
        {
            var sql = new StringBuilder();
            sql.Append(@"INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
            var batch = new List<string>();
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<MedicalRecordSnapshotDTO>(stream, cancellationToken: cancellationToken))
                {
                    if (record != null)
                    {
                        
                        int i = batch.Count;
                        batch.Add($"(@AppointmentId{i}, @Note{i}, @CreatedAt{i})");
                        parameters.Add($"AppointmentId{i}", record.AppointmentId);
                        parameters.Add($"Note{i}", record.Note);
                        parameters.Add($"CreatedAt{i}", record.CreatedAt);

                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
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
                    await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction, cancellationToken: cancellationToken));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing medical records: {ex.Message}");
                throw;
            }
        }
    }
}