using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MedicalData.Infrastructure.DTOs;
using Npgsql.Replication.PgOutput.Messages;
using MedicalData.Infrastructure.Helpers;

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
        
        public async Task ExportAppointmentsStatusesAsync(IDbConnection connection)
        {
            var sql = "select id as Id, status as Status from appointment_status";

            using var reader = await connection.ExecuteReaderAsync(sql);
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_appointment_status.json");
            await using var stream = File.Create(path);
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var appointmentStatus = new AppointmentStatusSnapshotDTO
                    {
                        Id = reader.GetInt32(idIndex),
                        Status = reader.GetString(statusIndex)
                    };
                    JsonSerializer.Serialize(writer, appointmentStatus);
                }
                writer.WriteEndArray();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error during exporting appointment_status table");
                throw;
            }
            finally
            {
                await writer.FlushAsync();
            }
           
        }
        public async Task InsertAppointmentStatusAsync(IDbConnection connection)
        {
            
            var path = Path.Combine(PathHelper.GetDataPath(), "exported_appointment_status.json");
            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<List<AppointmentStatusSnapshotDTO>>(json);
            var parameters = new DynamicParameters();
            if (data == null)
                throw new Exception("The file is empty");
            try
            {
                foreach (var item in data)
                {
                    var sql = "insert into appointment_status (id,status) values (@Id, @Status);";

                    await connection.ExecuteAsync(sql,item);

                   
                }
                var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('appointment_status','id'),
                    COALESCE((SELECT MAX(id) FROM appointment_status),1),
                    true
                    );";
                await connection.ExecuteAsync(resetSequenceSql);
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

