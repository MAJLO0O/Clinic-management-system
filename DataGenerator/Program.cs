
using DataGenerator.Data;
using DataGenerator.Generators;
using DataGenerator.Models;
using DataGenerator.Services;
using Microsoft.Extensions.Configuration;

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



Console.WriteLine("Test Data Generator");
Console.WriteLine("-------------------");
Console.WriteLine("1. Generate Doctors");
Console.WriteLine("2. Generate Patients");
Console.WriteLine("3. Generate Appointments");
string input = Console.ReadLine();
Console.Clear();
if (int.TryParse(input, out int choice))
{
    Console.WriteLine("How many records do you want to generate?");
    input = Console.ReadLine();
    if (input != null && int.TryParse(input, out int recordCount))
    {
        if(recordCount <= 0)
        {
            Console.WriteLine("Please enter a positive number.");
            return;
        }
        switch (choice)
        {
            case 1:
                Console.WriteLine("Inserting doctors...");
                await doctorSeederService.SeedDoctorsAsync(recordCount);
                Console.WriteLine("Success!");

                break;
            case 2:
                Console.WriteLine("Inserting patients...");
                await patientDataSeeder.SeedPatientsAsync(recordCount);
                Console.WriteLine("Success!");
                break;
                case 3:
                Console.WriteLine("Creating Appointments...");
                await appointmentDataSeeder.SeedAppointmentAsync(recordCount);
                Console.WriteLine("Success!");
                break;
            default:
                Console.WriteLine("Invalid choice. Please select 1 or 2.");
                break;
        }
    }
}
else
{
    Console.WriteLine("Invalid input. Please enter a number.");

}



