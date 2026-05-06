using DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalData.Infrastructure.Repositories;
using DataGenerator.Generators;
using System.Data;
namespace MedicalData.Aplication.Services
{
    public class SeedService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public SeedService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find postgreSql connection string");
        }
        public async Task SeedAllAsync(int recordCount)
        {
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
            var doctorSeedService = new DoctorSeederService(_configuration, new SpecializationRepository(), new DoctorRepository(), new DoctorSpecializationRepository(),
                new BranchRepository(), new DoctorGenereator(), new DoctorSpecializationGenerator());

            var appointmentSeedService = new AppointmentDataSeeder(_configuration, new AppointmentRepository(), new AppointmentStatusRepository(), new AppointmentGenerator(),
                new DoctorRepository(), new PatientRepository(), new MedicalRecordRepository(), new MedicalRecordGenerator(), new PaymentRepository(), new PaymentMethodRepository(),
                new PaymentStatusRepository(), new PaymentGenerator());

            var patientSeedService = new PatientDataSeeder( _configuration, new PatientGenerator(), new PatientRepository());
            try
            {

                await doctorSeedService.SeedDoctorsAsync(connection, transaction, recordCount);
                await patientSeedService.SeedPatientsAsync(connection, transaction, 2 * recordCount);
                await appointmentSeedService.SeedAppointmentAsync(connection, transaction, 5 * recordCount);
                transaction.Commit();
                Console.WriteLine("Seeded data");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error while seeding data {ex.Message}");
                throw;
            }
        }
    }
}
