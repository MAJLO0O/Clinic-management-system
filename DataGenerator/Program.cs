
using DataGenerator.Data;
using DataGenerator.Generators;
using DataGenerator.Models;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string connectionString = configuration.GetConnectionString("Postgres");


DoctorGenereator doctorGenereator = new();
PatientGenerator patientGenereator = new();
DoctorSpecializationGenerator doctorSpecializationGenerator = new();

DoctorRepository doctor = new (connectionString);
PersonRepository person = new (connectionString);
PatientRepository patient = new(connectionString);
SpecializationRepository specialization = new(connectionString);
DoctorSpecializationRepository doctorSpecialization = new(connectionString);


List<Doctor> doctors = new List<Doctor>();
List<Patient> patients = new List<Patient>();


await person.LoadExistingPesels(); 
await doctorSpecialization.LoadExistingDoctorsSpecializations();

var existingRelations = await doctorSpecialization.LoadExistingDoctorsSpecializations();

Console.WriteLine("Test Data Generator");
Console.WriteLine("-------------------");
Console.WriteLine("1. Generate Doctors");
Console.WriteLine("2. Generate Patients");
string input = Console.ReadLine();
Console.Clear();
if (int.TryParse(input, out int choice))
{
    Console.WriteLine("How many records do you want to generate?");
    input = Console.ReadLine();
    //TODO: validate input
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
                for (int i = 0; i < recordCount; i++)
                {
                    doctors.Add(doctorGenereator.GenerateDoctor());
                }
                await doctor.InsertDoctors(doctors);
                Console.WriteLine("New doctors inserted successfully!");
                var specializations = await specialization.GetExistingSpecializationIds(connectionString);
                var doctorIds = await doctor.GetExistingDoctorIdsWithoutSpecialization(connectionString);
                var newRelations = doctorSpecializationGenerator.GenerateDoctorSpecializationRelation(doctorIds, specializations, existingRelations);
                await doctorSpecialization.InsertDoctorSpecializations(newRelations);
                Console.WriteLine("New relations were made!");

                break;
            case 2:
                for (int i = 0; i < recordCount; i++)
                {
                    patients.Add(patientGenereator.GeneratePatient());
                }
                await patient.InsertPatients(patients);
                Console.WriteLine("Data inserted successfully!");

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



