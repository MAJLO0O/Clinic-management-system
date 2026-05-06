using DataGenerator.Generators;
using MedicalData.Domain.Models;
using DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Repositories;
using System.Diagnostics;
using MongoDB.Driver;

int recordCount()
{
    Console.WriteLine("How many records do you want to generate?");
    var input = Console.ReadLine();
    if (input != null && int.TryParse(input, out int recordCount))
    {
        while (recordCount <= 0)
        {
            Console.WriteLine("Please enter a positive number.");

        }
        return recordCount;
    }
    else
    {
        Console.WriteLine("Invalid input. Please enter a number.");
        return 0;
    }
}
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Connection string not found"); ;
string mongoConnectionString = configuration.GetConnectionString("MongoDb") ?? throw new Exception ("Mongo Connection string not found");
var connection = DbConnectionFactory.CreateDbConnection(connectionString);
MongoClient mongoClient = new MongoClient(mongoConnectionString);



DoctorGenereator doctorGenerator = new();
PatientGenerator patientGenerator = new();
DoctorSpecializationGenerator doctorSpecializationGenerator = new();
AppointmentGenerator appointmentGenerator = new();
MedicalRecordGenerator medicalRecordGenerator = new();
PaymentGenerator paymentGenerator = new();

DoctorRepository doctorRepository = new ();
PersonRepository personRepository = new ();
PatientRepository patientRepository = new();
SpecializationRepository specializationRepository = new();
BranchRepository branchRepository = new();
DoctorSpecializationRepository doctorSpecializationRepository = new();
AppointmentRepository appointmentRepository = new();
AppointmentStatusRepository appointmentStatusRepository = new();
MedicalRecordRepository medicalRecordRepository = new();
PaymentMethodRepository paymentMethodRepository = new();
PaymentStatusRepository paymentStatusRepository = new();
PaymentRepository paymentRepository = new();
MongoRepository mongoRepository = new(mongoClient);

List<Patient> patients = new List<Patient>();
List<Appointment> appointments = new List<Appointment>();

DoctorSeederService doctorSeederService = new(configuration, specializationRepository, doctorRepository, doctorSpecializationRepository,branchRepository ,doctorGenerator, doctorSpecializationGenerator);
PatientDataSeeder patientDataSeeder = new(configuration,patientGenerator,patientRepository);
AppointmentDataSeeder appointmentDataSeeder = new(configuration, appointmentRepository, appointmentStatusRepository, appointmentGenerator, doctorRepository, patientRepository, medicalRecordRepository,medicalRecordGenerator,paymentRepository ,paymentMethodRepository, paymentStatusRepository, paymentGenerator);
IndexBenchmarkService indexBenchmarkService = new(configuration, mongoClient);
DataCleanerService dataCleanerService = new(connectionString);


Console.WriteLine("Test Data Generator");
Console.WriteLine("-------------------");
Console.WriteLine("1. Generate Doctors");
Console.WriteLine("2. Generate Patients");
Console.WriteLine("3. Generate Appointments");
Console.WriteLine("4. Seed for benchmark");
Console.WriteLine("5. Benchmark");
string input = Console.ReadLine();
Console.Clear();
if (int.TryParse(input, out int choice))
{
    
        switch (choice)
        {
            
        case 1:
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                int count = recordCount();
                Console.WriteLine("Inserting doctors...");
                try
                {
                    await doctorSeederService.SeedDoctorsAsync(connection, transaction, count);
                    Console.WriteLine("Success!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding doctors: {ex.Message}");
                    transaction.Rollback();
                }
                break;
            }    
        case 2:
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                int count = recordCount();
                Console.WriteLine("Inserting patients...");
                try
                {
                    await patientDataSeeder.SeedPatientsAsync(connection, transaction, count);
                    transaction.Commit();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding patients: {ex.Message}");
                    transaction.Rollback();
                    Console.WriteLine("Success!");
                }
                break;
            }
                
        case 3:
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                int count = recordCount();
                Console.WriteLine("Creating Appointments...");
                try
                {
                    await appointmentDataSeeder.SeedAppointmentAsync(connection, transaction, count);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding appointments: {ex.Message}");
                    transaction.Rollback();
                }
                Console.WriteLine("Success!");
                break;
            }
                
        case 4:
            {
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                Stopwatch stopwatch = Stopwatch.StartNew();
                Console.WriteLine("Cleaning all data...");
                await dataCleanerService.ClearAllAsync();
                Console.WriteLine("Seeding data for benchmark... (number required is for number of doctors we will multiply it by 2 for patients and by 6 for appointments");
                int count = recordCount();
                Console.WriteLine("Seeding doctors");
                try
                {
                    await doctorSeederService.SeedDoctorsAsync(connection,transaction,count);
                    await patientDataSeeder.SeedPatientsAsync(connection,transaction,2 * count);
                    await appointmentDataSeeder.SeedAppointmentAsync(connection,transaction,5 * count);
                    stopwatch.Stop();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding data: {ex.Message}");
                    transaction.Rollback();
                }
                Console.WriteLine($"Data seeded in {stopwatch.Elapsed.TotalSeconds} seconds.");
                Console.WriteLine($"Successfully added {count + 2 * count + 5 * count} records!");

                break;
            } 
        case 5:
                Console.WriteLine("Benchmark");
                await indexBenchmarkService.BenchmarkWithIndexes();
                await indexBenchmarkService.BenchmarkWithoutIndexes();
                await indexBenchmarkService.BenchmarkMongoWithIndexes();
                await indexBenchmarkService.BenchmarkMongoWithoutIndexes();
            break;
        
        default:
                Console.WriteLine("Invalid choice. Please select one of the numbers");
        break;
        }
    }

else
{
    Console.WriteLine("Invalid input. Please enter a number.");

}



