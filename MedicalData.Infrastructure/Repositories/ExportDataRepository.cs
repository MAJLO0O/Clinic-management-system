using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using MedicalData.Infrastructure.DTOs;
using System.Xml.XPath;

namespace MedicalData.Infrastructure.Repositories
{
    public class ExportDataRepository
    {
       private readonly string _connectionString;
        public ExportDataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<AppointmentSnapshotDTO>> GetExportDataAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"select a.starting_date_time as StartingDateTime,
            d.first_name as DoctorFirstName,
            d.last_name as DoctorLastName,
            d.pesel as DoctorPesel,
            b.city as City,
            p.first_name as PatientFirstName,
            p.last_name as PatientLastName,
            p.pesel as PatientPesel
            from appointment a
            join doctor d on a.doctor_id = d.id 
            join branch b on d.branch_id = b.id
            join patient p on a.patient_id  = p.id";

            var result = await connection.QueryAsync<AppointmentSnapshotDTO>(sql);
            return result.ToList();
        }
    }
}
