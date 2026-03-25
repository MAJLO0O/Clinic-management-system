using Dapper;
using DataGenerator.Generators;
using DataGenerator.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DataGenerator.Data
{
    public class DoctorRepository
    {
        public async Task InsertDoctors(List<Doctor> doctors, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor (first_name, last_name,pesel,phone_number,email,branch_id) VALUES ");
            var parameters = new DynamicParameters();
            List<string> values = new();
           
                for (int i = 0; i < doctors.Count; i++)
                {
                    values.Add($"(@FirstName{i},@LastName{i},@Pesel{i},@PhoneNumber{i},@Email{i},@BranchId{i})");

                    parameters.Add($"FirstName{i}", doctors[i].FirstName);
                    parameters.Add($"LastName{i}", doctors[i].LastName);
                    parameters.Add($"Pesel{i}", doctors[i].Pesel);
                    parameters.Add($"PhoneNumber{i}", doctors[i].PhoneNumber);
                    parameters.Add($"Email{i}", doctors[i].Email);
                    parameters.Add($"BranchId{i}", doctors[i].BranchId);
                }

                sql.Append(string.Join(",", values));
                await connection.ExecuteAsync(sql.ToString(), parameters,transaction: transaction);

        }
        public async Task<List<int>> GetExistingDoctorsIds(IDbConnection connection,IDbTransaction transaction)
        {
                var sql = "select id from doctor";
                var doctorsIds = await connection.QueryAsync<int>(sql,transaction);
                return doctorsIds.ToList();
        }
        public async Task<List<int>> GetExistingDoctorIdsWithoutSpecialization(IDbConnection connection, IDbTransaction transaction)
        {
                var sql = "select d.id from doctor d left join doctor_specialization ds on d.id = ds.doctor_id where ds.specialization_id is null";
                var doctorIds = await connection.QueryAsync<int>(sql, transaction: transaction);
                return doctorIds.ToList();
        }
    }
}
