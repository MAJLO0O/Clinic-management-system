using Dapper;
using DataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class PatientRepository
    {

        public async Task InsertPatients(List<Patient> patients, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO patient (first_name, last_name,pesel,date_of_birth,phone,email) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            for (int i = 0; i < patients.Count; i++)
            {
                values.Add($"(@FirstName{i}, @LastName{i},@Pesel{i},@DateOfBirth{i},@PhoneNumber{i},@Email{i})");

                parameters.Add($"FirstName{i}", patients[i].FirstName);
                parameters.Add($"LastName{i}", patients[i].LastName);
                parameters.Add($"Pesel{i}", patients[i].Pesel);
                parameters.Add($"DateOfBirth{i}", patients[i].DateOfBirth);
                parameters.Add($"PhoneNumber{i}", patients[i].PhoneNumber);
                parameters.Add($"Email{i}", patients[i].Email);
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(sql.ToString(), parameters, transaction);
        }
        public async Task<List<int>> GetExistingPatientIds(IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "select id from patient";
            var patientIds = await connection.QueryAsync<int>(sql, transaction: transaction);
            return patientIds.ToList();
        }
    }
}
