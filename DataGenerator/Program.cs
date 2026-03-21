
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
AppointmentGenerator appointmentGenerator = new();
MedicalRecordGenerator medicalRecordGenerator = new();
PaymentGenerator paymentGenerator = new();

DoctorRepository doctor = new (connectionString);
PersonRepository person = new (connectionString);
PatientRepository patient = new(connectionString);
SpecializationRepository specialization = new(connectionString);
BranchRepository branch = new(connectionString);
DoctorSpecializationRepository doctorSpecialization = new(connectionString);
AppointmentRepository appointment = new(connectionString);
AppointmentStatusRepository appointmentStatus = new(connectionString);
MedicalRecordRepository medicalRecord = new(connectionString);
PaymentMethodRepository paymentMethod = new(connectionString);
PaymentStatusRepository paymentStatus = new(connectionString);
PaymentRepository payment = new(connectionString);


List<Doctor> doctors = new List<Doctor>();
List<Patient> patients = new List<Patient>();
List<Appointment> appointments = new List<Appointment>();


await person.LoadExistingPesels();
await doctorSpecialization.LoadExistingDoctorsSpecializations();



var existingRelations = await doctorSpecialization.LoadExistingDoctorsSpecializations();
var branchIds = await branch.branchIds();

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
                for (int i = 0; i < recordCount; i++)
                {
                    doctors.Add(doctorGenereator.GenerateDoctor(branchIds));
                }
                await doctor.InsertDoctors(doctors);
                Console.WriteLine("New doctors inserted successfully!");
                var specializations = await specialization.GetExistingSpecializationIds();
                var doctorIds = await doctor.GetExistingDoctorIdsWithoutSpecialization();
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
                case 3:
                var existingAppointments = await appointment.GetExistingAppointments();
                var appointmentStatusIds = await appointmentStatus.GetExistingAppointmentStatusIds();
                var doctorIdsForAppointments = await doctor.GetExistingDoctorsIds();
                var patientIdsForAppointments = await patient.GetExistingPatientIds();
                for (int i = 0; i < recordCount; i++)
                {
                    appointments.Add(appointmentGenerator.GenerateAppointment(doctorIdsForAppointments, patientIdsForAppointments, appointmentStatusIds, existingAppointments));
                }
                await appointment.InsertAppointments(appointments);
                var appointmentsWithoutMedicalRecord = await appointment.GetAppointmentIdsWithoutMedicalRecordAndWhenTheyWereCreated();
                var medicalRecords = medicalRecordGenerator.GenerateMedicalRecords(appointmentsWithoutMedicalRecord);
                await medicalRecord.InsertMedicalRecords(medicalRecords);
                var appointmentsWithoutPayment = await appointment.GetAppointmetsWithoutPayment();
                var paymentMethods = await paymentMethod.GetExistingPaymentMethodIds();
                var paymentStatusIds = await paymentStatus.GetExistingPaymentStatusIds();
                var usedPaymentNumber = await payment.GetExistingPaymentNumbers();
                var payments = paymentGenerator.GeneratePayments(appointmentsWithoutPayment,paymentMethods,paymentStatusIds,usedPaymentNumber);
                await payment.InsertPayments(payments);
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



