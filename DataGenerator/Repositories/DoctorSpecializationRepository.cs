using Dapper;
using DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class DoctorSpecializationRepository
    {
        public async Task<HashSet<(int, int)>> LoadExistingDoctorsSpecializations(IDbConnection connection, IDbTransaction transaction)
        {
            var result = new HashSet<(int, int)>();
            var sql = "select doctor_id,specialization_id from doctor_specialization";
            var rows = await connection.QueryAsync<(int doctorId, int specializationId)>(sql, transaction: transaction);
            foreach (var row in rows)
            {
                result.Add((row.doctorId, row.specializationId));
            }
            return result;

        }
        

        public async Task InsertDoctorSpecializations(List<(int doctorId, int specializationId)> newRelations, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            int i = 0;
            foreach (var relation in newRelations)
            {
                values.Add($"(@DoctorId{i}, @SpecializationId{i})");

                parameters.Add($"DoctorId{i}", relation.doctorId);
                parameters.Add($"SpecializationId{i}", relation.specializationId);
                i++;
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters,transaction: transaction);
        } 
    }
}
