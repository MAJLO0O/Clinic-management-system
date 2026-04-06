
using DataGenerator.Data;
using DataGenerator.Generators;
using MedicalData.Domain.Models;
using DataGenerator.Services;
using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Repositories;

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

string connectionString = configuration.GetConnectionString("Postgres");


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




List<Patient> patients = new List<Patient>();
List<Appointment> appointments = new List<Appointment>();

DoctorSeederService doctorSeederService = new(connectionString, specializationRepository, doctorRepository, doctorSpecializationRepository,branchRepository ,doctorGenerator, doctorSpecializationGenerator);
PatientDataSeeder patientDataSeeder = new(connectionString,patientGenerator,patientRepository);
AppointmentDataSeeder appointmentDataSeeder = new(connectionString, appointmentRepository, appointmentStatusRepository, appointmentGenerator, doctorRepository, patientRepository, medicalRecordRepository,medicalRecordGenerator,paymentRepository ,paymentMethodRepository, paymentStatusRepository, paymentGenerator);
IndexBenchmarkService indexBenchmarkService = new(connectionString);
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
                int count = recordCount();        
                Console.WriteLine("Inserting doctors...");
                await doctorSeederService.SeedDoctorsAsync(count);
                Console.WriteLine("Success!");

        break;
            
        case 2:
                count = recordCount();
                Console.WriteLine("Inserting patients...");
                await patientDataSeeder.SeedPatientsAsync(count);
                Console.WriteLine("Success!");
        break;
                
        case 3:
                count = recordCount();
                Console.WriteLine("Creating Appointments...");
                await appointmentDataSeeder.SeedAppointmentAsync(count);
                Console.WriteLine("Success!");
        break;
                
        case 4:
                Console.WriteLine("Cleaning all data...");
                await dataCleanerService.ClearAllAsync();
                Console.WriteLine("Seeding data for benchmark... (number required is for number of doctors we will multiply it by 2 for patients and by 6 for appointments");
                count = recordCount();
                await doctorSeederService.SeedDoctorsAsync(count);
                await patientDataSeeder.SeedPatientsAsync(2*count);
                await appointmentDataSeeder.SeedAppointmentAsync(6*count);
                Console.WriteLine($"Successfully added {count+2*count+6*count} records!");
            break;
                
        case 5:
                Console.WriteLine("Benchmark");
                await indexBenchmarkService.BenchmarkWithIndexes();
                await indexBenchmarkService.BenchmarkWithoutIndexes();
        break;
        
        default:
                Console.WriteLine("Invalid choice. Please select 1 or 2.");
        break;
        }
    }

else
{
    Console.WriteLine("Invalid input. Please enter a number.");

}



