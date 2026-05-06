using MedicalData.Import.Services;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Mappers;
using MedicalData.Import.ZipReader;
using MongoDB.Driver;


var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false)
           .Build();
var mongoConnectionString = configuration.GetConnectionString("MongoDb") ?? throw new InvalidOperationException("Could find mogo connection string");

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
ManifestReader manifestReader = new();
MongoRepository mongoRepository = new(new MongoClient(configuration.GetConnectionString("MongoDb")));

MedicalDataImportService importService = new(configuration,doctorRepository, patientRepository, appointmentRepository,
    importDataRepository,appointmentStatusRepository,branchRepository, doctorSpecializationRepository,
    specializationRepository,paymentMethodRepository,paymentStatusRepository,medicalRecordRepository, paymentRepository, manifestReader,mongoRepository);

AppointmentMapper appointmentMapper = new();
ExportDataRepository exportDataRepository = new(configuration);

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
            {
                Console.WriteLine("Importing Data...");

                using var cts = new CancellationTokenSource();

                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("Cancelling...");
                    e.Cancel = true;
                    cts.Cancel();
                };
                await importService.ImportDataFromJson(cts.Token);
                Console.WriteLine("Data Imported!!!");
                break;
            }
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

