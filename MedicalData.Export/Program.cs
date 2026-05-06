using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Repositories;
using MedicalData.Export.Services;
using System.Diagnostics;
using MedicalData.Export.Manifest;
namespace MedicalData.Export
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
            var connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find connections string");

            ExportDataRepository exportDataRepository = new(configuration);
            AppointmentRepository appointmentRepository = new();
            AppointmentStatusRepository appointmentStatusRepository = new();
            BranchRepository branchRepository = new();
            DoctorRepository doctorRepository = new();
            DoctorSpecializationRepository doctorSpecializationRepository = new();
            MedicalRecordRepository medicalRecordRepository = new();
            PaymentMethodRepository paymentMethodRepository = new();
            PaymentRepository paymentRepository = new();
            PaymentStatusRepository paymentStatusRepository = new();
            PatientRepository patientRepository = new();
            SpecializationRepository specializationRepository = new();
            ExportManifestBuilder manifestBuilder = new();

            ExportDataService exportDataService = new(configuration,exportDataRepository,appointmentRepository,appointmentStatusRepository,
                branchRepository,doctorRepository,doctorSpecializationRepository,medicalRecordRepository, paymentMethodRepository,
                paymentRepository, patientRepository,specializationRepository, paymentStatusRepository, manifestBuilder);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Export started");
            
            await exportDataService.ExportAppointmentsLocalAsync();
            stopwatch.Stop();
            Console.WriteLine($"Export completed in {stopwatch.Elapsed.TotalSeconds} seconds");
            Console.WriteLine("Export completed");
        }
    }
}
