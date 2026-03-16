using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataGenerator.Generators;
using DataGenerator.Models;
using Npgsql;

namespace DataGenerator.Data
{
    internal class DoctorRepository
    {
        private readonly string _connectionString;
        public DoctorRepository(string connectionString)
        {
            _connectionString = connectionString;
        }



        public async Task InsertDoctors(List<Doctor> doctors)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor (first_name, last_name,pesel,phone_number,email) VALUES ");
            var parameters = new DynamicParameters();
            List<string> values = new();
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                for (int i = 0; i < doctors.Count; i++)
                {
                    values.Add($"(@FirstName{i},@LastName{i},@Pesel{i},@PhoneNumber{i},@Email{i})");

                    parameters.Add($"FirstName{i}", doctors[i].FirstName);
                    parameters.Add($"LastName{i}", doctors[i].LastName);
                    parameters.Add($"Pesel{i}", doctors[i].Pesel);
                    parameters.Add($"PhoneNumber{i}", doctors[i].PhoneNumber);
                    parameters.Add($"Email{i}", doctors[i].Email);


                }

                sql.Append(string.Join(",", values));
                Console.WriteLine(sql.ToString());
                await connection.ExecuteAsync(sql.ToString(), parameters);
            }

        }
        public async Task<List<int>> GetExistingDoctorIdsWithoutSpecialization(string connectionString)
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(connectionString))
            {
                await connection.OpenAsync();
                var sql = "select d.id from doctor d left join doctor_specialization ds on d.id = ds.doctor_id where ds.specialization_id is null";
                var doctorIds = await connection.QueryAsync<int>(sql);
                return doctorIds.ToList();
            }
        }
    }
}
