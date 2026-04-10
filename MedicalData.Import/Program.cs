using MedicalData.Infrastructure.DTOs;
using MedicalData.Import.Services;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using DataGenerator.Data;
using MedicalData.Infrastructure.Mappers;


var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .Build();
string connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Connection string not found");
string mongoConnectionString = configuration.GetConnectionString("MongoDB") ?? throw new Exception("MongoDB Connection string not found");


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

AppointmentMapper appointmentMapper = new();
ExportDataRepository exportDataRepository = new(connectionString);

MongoImportService mongoImportService = new(mongoConnectionString, appointmentMapper, exportDataRepository);
Console.WriteLine("Data Import Program");
Console.WriteLine("-------------------");
Console.WriteLine("Choose operation");
Console.WriteLine("1. Import Data from Json to Postgres");
Console.WriteLine("2. Import Data from Postgres to MongoDB");
var input = Console.ReadLine();
if(input is not null && int.TryParse(input, out int operation))
{
    switch (operation)
    {
        case 1:
             Console.WriteLine("Importing Data...");
            await importService.ImportDataFromJson();
             Console.WriteLine("Data Imported!!!");
            break;
        case 2:
             Console.WriteLine("Importing Data to MongoDB...");
             await mongoImportService.ImportDataToMongoDB();
             Console.WriteLine("Data Imported to MongoDB!!!");
            break;
        default:
             Console.WriteLine("Invalid operation. Please choose either 1 or 2.");
            break;
    }
}
else
{
     Console.WriteLine("Invalid input. Please enter a number.");
    return;
}

