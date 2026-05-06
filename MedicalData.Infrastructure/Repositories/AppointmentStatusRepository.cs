using Dapper;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using Npgsql.Replication.PgOutput.Messages;
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
    public class AppointmentStatusRepository
    {
        public async Task<List<int>> GetExistingAppointmentStatusIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from appointment_status";
            var statusIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return statusIds.ToList();
        }
        
        public async Task WriteAppointmentStatusesJsonAsync(IDbConnection connection, Stream stream , CancellationToken ct)
        {
            var sql = "select id as Id, status as Status from appointment_status";

            using var reader = await connection.ExecuteReaderAsync(sql);
            await using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");

                writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();

                    var appointmentStatus = MapToAppointmentStatus(reader,idIndex, statusIndex);
                    JsonSerializer.Serialize(writer, appointmentStatus);
                }
                writer.WriteEndArray();

                await writer.FlushAsync();
           
        }
        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var path = Path.Combine(Path.GetTempPath(), "exported_appointment_status.json");
            
            using var stream= File.Create(path);

            await WriteAppointmentStatusesJsonAsync(connection, stream, ct);
        }
        public async Task AddAppointmetStasusesToZipAsync(IDbConnection connection, ZipArchive zip ,CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_appointment_status.json");

            await using var entryStream = entry.Open();
            await WriteAppointmentStatusesJsonAsync(connection, entryStream, ct);
        }
        public AppointmentStatusSnapshotDTO MapToAppointmentStatus(IDataReader reader, int idIndex, int statusIndex)
        {
             return new AppointmentStatusSnapshotDTO
            {
                Id = reader.GetInt32(idIndex),
                Status = reader.GetString(statusIndex)
            };
        }

        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_appointment_status.json");
            using var stream = File.OpenRead(path);

            await ImportAppointmentStatusAsync(connection, transaction, stream, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_appointment_status.json");
            if (entry == null)
                throw new InvalidOperationException("exported_appointments file is null");

            using var stream = entry.Open();
            await ImportAppointmentStatusAsync(connection, transaction, stream, cancellationToken);

        }


        public async Task ImportAppointmentStatusAsync(IDbConnection connection,IDbTransaction transaction, Stream stream,CancellationToken ct)
        {

            var data = await JsonSerializer.DeserializeAsync<List<AppointmentStatusSnapshotDTO>>(stream, cancellationToken: ct);
            var parameters = new DynamicParameters();
            if (data == null)
                throw new Exception("The file is empty");
            try
            {
                foreach (var item in data)
                {
                    var sql = "insert into appointment_status (id,status) values (@Id, @Status);";

                    await connection.ExecuteAsync(sql,item,transaction: transaction);

                   
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('appointment_status','id'),
                    COALESCE((SELECT MAX(id) FROM appointment_status),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql,transaction: transaction);
                Console.WriteLine("Imported appointment_status");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed inserting to table appointment_status {ex.Message}");
                throw;
            }
            
        }
    }
    }

