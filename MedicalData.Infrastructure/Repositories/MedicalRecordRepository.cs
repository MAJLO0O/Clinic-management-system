using Dapper;
using MedicalData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MedicalData.Infrastructure.Repositories
{
    public class MedicalRecordRepository
    {
        public async Task InsertMedicalRecords(List<MedicalRecord> medicalRecords,IDbConnection connection,IDbTransaction transaction)
        {
                var sql = new StringBuilder();
                sql.Append("INSERT INTO medical_record (appointment_id, note, created_at) VALUES ");
                var parameters = new DynamicParameters();
                var values = new List<string>();
                for(int i=0; i < medicalRecords.Count; i++)
                {
                    values.Add($"( @AppointmentId{i}, @Note{i}, @CreatedAt{i})");
                    
                    parameters.Add($"@AppointmentId{i}", medicalRecords[i].AppointmentId);
                    parameters.Add($"@Note{i}", medicalRecords[i].Note);
                    parameters.Add($"@CreatedAt{i}", medicalRecords[i].CreatedAt);

                    
                }
                sql.Append(string.Join(",", values));
                await connection.ExecuteAsync(sql.ToString(), parameters,transaction);
        }
    }
}
