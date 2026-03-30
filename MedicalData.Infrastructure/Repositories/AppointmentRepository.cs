using Dapper;
using MedicalData.Domain.Models;
using System.Data;
using System.Text;

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
            
    }

