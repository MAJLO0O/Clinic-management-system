using DataGenerator.Data;
using DataGenerator.Generators;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Services
{
    public class PatientDataSeeder
    {
       
        private readonly string _connectionString;
        private PatientGenerator _patientGenerator;
        private readonly PatientRepository _patientRepository;
        public PatientDataSeeder(string connectionString,PatientGenerator patientGenerator, PatientRepository patientRepository)
        {
            _connectionString = connectionString;
            _patientGenerator = patientGenerator;
            _patientRepository = patientRepository;
        }
        public async Task SeedPatientsAsync(int recordCount)
        {
            List<Patient> patients = new List<Patient>();
            using NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                int globalIndex = 0;
                while(recordCount > 0)
                {
                    int batchSize = Math.Min(5000, recordCount);
                    for (int i = 0; i < batchSize; i++)
                    {
                        patients.Add(_patientGenerator.GeneratePatient(globalIndex));
                        globalIndex++;
                    }
                    await _patientRepository.InsertPatients(patients, connection, transaction);
                    patients.Clear();
                    recordCount -= batchSize;
                }
            Console.WriteLine("Patients inserted successfully!");
            await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting Patients: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }

        }
         
    }
}
