using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using Npgsql.Internal;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace MedicalData.Infrastructure.Repositories;

public class AppointmentRepository
{
   
    public async Task<HashSet<(int DoctorId,DateTime StartingDateTime)>> GetExistingAppointments(IDbConnection connection,IDbTransaction transaction)
        {
                var sql = "select doctor_id,starting_date_time from appointment";
                var appointmentsIds = await connection.QueryAsync<(int DoctorId,DateTime StartingDateTime)>(sql,transaction: transaction);
                return appointmentsIds.ToHashSet();
        }
    public async Task<List<(int, DateTime)>> GetAppointmentIdsWithoutMedicalRecordAndWhenTheyWereCreated(IDbConnection connection, IDbTransaction transaction)
    {
            var sql = @"
                select a.id,a.starting_date_time
                from appointment a
                left join medical_record mr on a.id=mr.appointment_id
                where mr.appointment_id is null
                AND a.starting_date_time < NOW()";
            var ids = await connection.QueryAsync<(int id, DateTime CreatedAt)>(sql, transaction: transaction);
            return ids.ToList();
    }
        public async Task<List<int>> GetAppointmentsWithoutPayment(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = @"
                    select a.id
                    from appointment a
                    left join payment p on a.id=p.appointment_id
                    where p.appointment_id is null
                    AND a.starting_date_time < NOW()";
                var ids = await connection.QueryAsync<int>(sql, transaction: transaction);
                return ids.ToList();
            }
    
    public async Task InsertAppointments(List<Appointment> appointments,IDbConnection connection,IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("insert into appointment (starting_date_time, doctor_id, patient_id, appointment_status_id) values ");
            var parameters = new DynamicParameters();
            var values = new List<string>();

        
                    for(int i=0;i<appointments.Count;i++)
                    {
                    values.Add($"(@StartingDateTime{i},@DoctorId{i},@PatientId{i},@AppointmentStatusId{i})");

                    parameters.Add($"StartingDateTime{i}", appointments[i].StartingDateTime);
                    parameters.Add($"DoctorId{i}", appointments[i].DoctorId);
                    parameters.Add($"PatientId{i}", appointments[i].PatientId);
                    parameters.Add($"AppointmentStatusId{i}", appointments[i].AppointmentStatusId);
                    }
        sql.Append(string.Join(",", values));
                await connection.ExecuteAsync(sql.ToString(), parameters,transaction);
        }

    public async Task InsertAppointmentsFromImport(int DoctorId, int PatientId, DateTime StartingDateTime, IDbConnection connection, IDbTransaction transaction)
    {
        var sql = @"insert into appointment (starting_date_time, doctor_id, patient_id) values (@StartingDateTime,@DoctorId,@PatientId)";
        await connection.ExecuteAsync(sql,
            new
            {
                StartingDateTime,
                DoctorId,
                PatientId,
            }, transaction);
    }
    public async Task ExportAllDataFromAppointmet(IDbConnection connection)
    {
        var sql = @"select id as Id, starting_date_time as StartingDateTime,
        created_at as CreatedAt, doctor_id as DoctorId, patient_id as PatientId,
        appointment_status_id as AppointmentStatusId from appointment";
        using var reader = await connection.ExecuteReaderAsync(sql);
        var path = Path.Combine(PathHelper.GetDataPath(), "exported_appointments.json");
        await using var stream = File.Create(path);
        using var writer = new Utf8JsonWriter(stream);

        var idIndex = reader.GetOrdinal("Id");
        var startIndex = reader.GetOrdinal("StartingDateTime");
        var createdIndex = reader.GetOrdinal("CreatedAt");
        var doctorIndex = reader.GetOrdinal("DoctorId");
        var patientIndex = reader.GetOrdinal("PatientId");
        var statusIndex = reader.GetOrdinal("AppointmentStatusId");

        try
        {
            writer.WriteStartArray();
            while (reader.Read())
            {
                var appointment = new AppointmentSnapshotDTO
                {

                    Id = reader.GetInt32(idIndex),
                    StartingDateTime = reader.GetDateTime(startIndex),
                    CreatedAt = reader.GetDateTime(createdIndex),
                    DoctorId = reader.GetInt32(doctorIndex),
                    PatientId = reader.GetInt32(patientIndex),
                    AppointmentStatusId = reader.GetInt32(statusIndex)
                };

                JsonSerializer.Serialize(writer, appointment);
            }
            writer.WriteEndArray();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while exporting appointments: {ex.Message}");
            throw;
        }
        finally
        {
            await writer.FlushAsync();
        }
    }
    public async Task ImportAppointmentAsync(IDbConnection connection)
    {
        var path = Path.Combine(PathHelper.GetDataPath(), "exported_appointments.json");
        var stream = File.OpenRead(path);
        var sql = new StringBuilder();
        sql.Append(@"insert into appointment (id, starting_date_time, created_at, doctor_id, patient_id, appointment_status_id) values ");
        var batch = new List<string>();
        var parameters = new DynamicParameters();
        using var transaction = connection.BeginTransaction();
        try
        {
            await foreach (var appointment in JsonSerializer.DeserializeAsyncEnumerable<AppointmentSnapshotDTO>(stream))
            {
                if (appointment != null)
                {
                    int i = batch.Count;
                    batch.Add($"(@Id{i}, @StartingDateTime{i}, @CreatedAt{i}, @DoctorId{i}, @PatientId{i}, @AppointmentStatusId{i})");
                    parameters.Add($"Id{i}", appointment.Id);
                    parameters.Add($"StartingDateTime{i}", appointment.StartingDateTime);
                    parameters.Add($"CreatedAt{i}", appointment.CreatedAt);
                    parameters.Add($"DoctorId{i}", appointment.DoctorId);
                    parameters.Add($"PatientId{i}", appointment.PatientId);
                    parameters.Add($"AppointmentStatusId{i}", appointment.AppointmentStatusId);
                    if (batch.Count == 5000)
                    {
                        sql.Append(string.Join(",", batch));
                        await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
                        batch.Clear();
                        sql = new StringBuilder();
                        sql.Append(@"insert into appointment (id, starting_date_time, created_at, doctor_id, patient_id, appointment_status_id) values ");
                        parameters = new DynamicParameters();
                    }
                }
            }
            if (batch.Count > 0)
            {
                sql.Append(string.Join(",", batch));
                await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
            }
            var resetSequenceSql = @"SELECT setval(
                    pg_get_serial_sequence('appointment','id'),
                    COALESCE((SELECT MAX(id) FROM appointment),1),
                    true
                    );";
            await connection.ExecuteAsync(resetSequenceSql);
            transaction.Commit();
            Console.WriteLine("Imported appointment");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"An error occurred while importing appointments: {ex.Message}");
            throw;
        }
    }
}

