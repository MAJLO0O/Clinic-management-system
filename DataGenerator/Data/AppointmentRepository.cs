using Dapper;
using DataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class AppointmentRepository
    {
            private readonly string _connectionString;
            public AppointmentRepository(string connectionString)
            {
                _connectionString = connectionString;
            }
            
            public async Task<HashSet<(int DoctorId,DateTime StartngDateTime)>> GetExistingAppointmentsIds()
            {
                using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select doctor_id,starting_date_time from appointment";
                    var appointmentsIds = await connection.QueryAsync<(int doctorId,DateTime StartingDateTime)>(sql);
                    return appointmentsIds.ToHashSet();
                }

            
            }
            
            public async Task InsertAppointments(List<Appointment> appointments)
            {
                var sql = new StringBuilder();
                sql.Append("insert into appointment (starting_date_time, doctor_id, patient_id, appointment_status_id) values ");
                var parameters = new DynamicParameters();
                var values = new List<string>();
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
            
                        for(int i=0;i<appointments.Count;i++)
                        {
                        values.Add($"(@StartingDateTime{i},@DoctorId{i},@PatientId{i},@AppointmentStatusId{i})");

                        parameters.Add($"StartingDateTime{i}", appointments[i].StartingDateTime);
                        parameters.Add($"DoctorId{i}", appointments[i].DoctorId);
                        parameters.Add($"PatientId{i}", appointments[i].PatientId);
                        parameters.Add($"AppointmentStatusId{i}", appointments[i].AppointmentStatusId);
                        }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters);
            }
                
        }
    }
}
