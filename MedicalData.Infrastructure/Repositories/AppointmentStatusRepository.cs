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
            await using var stream = File.Create("exported_appointment_status.json");
            using var writer = new Utf8JsonWriter(stream);

            var idIndex = reader.GetOrdinal("Id");
            var statusIndex = reader.GetOrdinal("Status");
            try
            {
                writer.WriteStartArray();
                while (reader.Read())
                {
                    var appointmentStatus = new ExportAppointmentStatusDTO
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
    }
    }

