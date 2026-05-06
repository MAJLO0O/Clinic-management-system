using DataGenerator.Generators;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace DataGenerator.Services
{
    public class PatientDataSeeder
    {
       
        private readonly string _connectionString;
        private PatientGenerator _patientGenerator;
        private readonly PatientRepository _patientRepository;
        public PatientDataSeeder(IConfiguration configuration,PatientGenerator patientGenerator, PatientRepository patientRepository)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Coulnd't find connection string");
            _patientGenerator = patientGenerator;
            _patientRepository = patientRepository;
        }
        public async Task SeedPatientsAsync(IDbConnection connection, IDbTransaction transaction, int recordCount)
        {
            List<Patient> patients = new List<Patient>();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting Patients: {ex.Message}");
                throw;
            }

        }
         
    }
}
