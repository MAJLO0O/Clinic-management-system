using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using MedicalData.Infrastructure.DTOs;
using System.Xml.XPath;
using MedicalData.Infrastructure.NoSqlDTOs;

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

        public async Task<List<AppointmentExport>> ExportAppointmentToNoSqlAsync(int offset, int chunk)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = @"select a.id as AppointmentId,
            a.starting_date_time as StartingDateTime,
            ast.status as AppointmentStatus, 
            mr.note as MedicalRecordNote,
            mr.created_at as MedicalRecordCreatedAt,
            pay.id as PaymentId,
            pay.payment_number as PaymentNumber,
            pay.amount as PaymentAmount,
            pm.method as PaymentMethod,
            ps.status as PaymentStatus,
            d.id as DoctorId,
            d.first_name as DoctorFirstName,
            d.last_name as DoctorLastName,
            d.pesel as DoctorPesel,
            ARRAY_AGG(distinct s.name) filter (where s.name is not null) as DoctorSpecializations,
            b.id as DoctorBranchId,
            b.city as DoctorBranchCity,
            b.address as DoctorBranchAddress,
            p.id as PatientId,
            p.first_name as PatientFirstName,
            p.last_name as PatientLastName,
            p.pesel as PatientPesel
            from appointment a
            join doctor d on a.doctor_id = d.id 
            join branch b on d.branch_id = b.id
            join patient p on a.patient_id  = p.id
            left join payment pay on a.id = pay.appointment_id
            left join payment_status ps on pay.payment_status_id = ps.id
            left join payment_method pm on pay.payment_method_id = pm.id
            left join doctor_specialization ds on d.id = ds.doctor_id
            left join specialization s on ds.specialization_id = s.id
            left join medical_record mr on a.id = mr.appointment_id
            join appointment_status ast on a.appointment_status_id = ast.id
            group by a.id, a.starting_date_time, ast.status, mr.note, mr.created_at, pay.id,
            pay.payment_number, pay.amount, pm.method, ps.status, d.id, d.first_name, d.last_name,
            d.pesel, b.id, b.city, b.address, p.id, p.first_name, p.last_name, p.pesel
            order by a.id
            offset @offset rows
            fetch next @chunk rows only";

            var result = await connection.QueryAsync<AppointmentExport>(sql, new { offset, chunk });
            return result.ToList();
        }
    }
}
