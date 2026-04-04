using MedicalData.Infrastructure.DTOs;
using MedicalData.Import.Services;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;


var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .Build();
string connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Connection string not found");

DoctorRepository doctorRepository = new();
PatientRepository patientRepository = new();
AppointmentRepository appointmentRepository = new();

MedicalDataImportService importService = new(doctorRepository, patientRepository, appointmentRepository);

Console.WriteLine("Importing Data...");

//to-do dodac w serwisie funkcje odpowiedzialna za workflow
