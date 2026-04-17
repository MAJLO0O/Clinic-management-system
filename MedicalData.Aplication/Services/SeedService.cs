using DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalData.Infrastructure.Repositories;
using DataGenerator.Generators;
using DataGenerator.Data;
namespace MedicalData.Aplication.Services
{
    public class SeedService
    {
        private readonly string _connectionString;

        public SeedService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find postgreSql connection string");
        }
        public async Task SeedAllAsync(int recordCount)
        {
            var doctorSeedService = new DoctorSeederService(_connectionString, new SpecializationRepository(), new DoctorRepository(), new DoctorSpecializationRepository(),
                new BranchRepository(), new DoctorGenereator(), new DoctorSpecializationGenerator());

            var appointmentSeedService = new AppointmentDataSeeder(_connectionString, new AppointmentRepository(), new AppointmentStatusRepository(), new AppointmentGenerator(),
                new DoctorRepository(), new PatientRepository(), new MedicalRecordRepository(), new MedicalRecordGenerator(), new PaymentRepository(), new PaymentMethodRepository(),
                new PaymentStatusRepository(), new PaymentGenerator());

            var patientSeedService = new PatientDataSeeder(_connectionString, new PatientGenerator(), new PatientRepository());

            await doctorSeedService.SeedDoctorsAsync(recordCount);
            await patientSeedService.SeedPatientsAsync(2*recordCount);
            await appointmentSeedService.SeedAppointmentAsync(5*recordCount);
        }
    }
}
