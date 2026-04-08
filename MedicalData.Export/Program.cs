using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Repositories;
using MedicalData.Export.Services;
using DataGenerator.Data;
using System.Diagnostics;
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
            string connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Connection string not found");


            ExportDataRepository exportDataRepository = new(connectionString);
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

            ExportDataService exportDataService = new(connectionString,exportDataRepository,appointmentRepository,appointmentStatusRepository,
                branchRepository,doctorRepository,doctorSpecializationRepository,medicalRecordRepository, paymentMethodRepository,
                paymentRepository, patientRepository,specializationRepository, paymentStatusRepository);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Export started");
            
            await exportDataService.ExportAppointmentsAsync();
            stopwatch.Stop();
            Console.WriteLine($"Export completed in {stopwatch.Elapsed.TotalSeconds} seconds");
            Console.WriteLine("Export completed");
        }
    }
}
