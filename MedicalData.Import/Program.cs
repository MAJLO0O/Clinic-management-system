using MedicalData.Infrastructure.DTOs;
using MedicalData.Import.Services;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using DataGenerator.Data;


var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .Build();
string connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Connection string not found");


DoctorRepository doctorRepository = new();
PatientRepository patientRepository = new();
AppointmentRepository appointmentRepository = new();
ImportDataRepository importDataRepository = new();
AppointmentStatusRepository appointmentStatusRepository = new();
BranchRepository branchRepository = new();
SpecializationRepository specializationRepository = new();
DoctorSpecializationRepository doctorSpecializationRepository = new();
PaymentMethodRepository paymentMethodRepository = new();
PaymentStatusRepository paymentStatusRepository = new();
MedicalRecordRepository medicalRecordRepository = new();
PaymentRepository paymentRepository = new();

MedicalDataImportService importService = new(connectionString,doctorRepository, patientRepository, appointmentRepository,
    importDataRepository,appointmentStatusRepository,branchRepository, doctorSpecializationRepository,
    specializationRepository,paymentMethodRepository,paymentStatusRepository,medicalRecordRepository, paymentRepository);

Console.WriteLine("Importing Data...");
await importService.ImportDataFromJson();
Console.WriteLine("Data Imported!!!");
