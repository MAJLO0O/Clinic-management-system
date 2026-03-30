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
            for (int i = 0; i<recordCount; i++)
            {
                patients.Add(_patientGenerator.GeneratePatient(i));
            }
                foreach (var chunk in patients.Chunk(1000))
                {
                    await _patientRepository.InsertPatients(chunk.ToList(), connection, transaction);
                }
            Console.WriteLine("Data inserted successfully!");
            await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting data: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }

        }
         
    }
}
