using DataGenerator.Generators;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace DataGenerator.Services
{
    public class AppointmentDataSeeder
    {
        public readonly string _connectionString;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly AppointmentStatusRepository _appointmentStatusRepository;
        private readonly AppointmentGenerator _appointmentGenerator;
        private readonly DoctorRepository _doctorRepository;
        private readonly PatientRepository _patientRepository;
        private readonly MedicalRecordRepository _medicalRecordRepository;
        private readonly MedicalRecordGenerator _medicalRecordGenerator;
        private readonly PaymentRepository _paymentRepository;
        private readonly PaymentMethodRepository _paymentMethodRepository;
        private readonly PaymentStatusRepository _paymentStatusRepository;
        private readonly PaymentGenerator _paymentGenerator;

        public AppointmentDataSeeder(IConfiguration configuration,
            AppointmentRepository appointmentRepository, AppointmentStatusRepository appointmentStatusRepository,
            AppointmentGenerator appointmentGenerator, DoctorRepository doctorRepository, PatientRepository patientRepository,
            MedicalRecordRepository medicalRecordRepository,MedicalRecordGenerator medicalRecordGenerator , PaymentRepository paymentRepository, PaymentMethodRepository paymentMethodRepository,
            PaymentStatusRepository paymentStatusRepository, PaymentGenerator paymentGenerator)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Coulnd't find connection string");
            _appointmentRepository = appointmentRepository;
            _appointmentStatusRepository = appointmentStatusRepository;
            _appointmentGenerator = appointmentGenerator;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _medicalRecordGenerator = medicalRecordGenerator;
            _paymentRepository = paymentRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentStatusRepository = paymentStatusRepository;
            _paymentGenerator = paymentGenerator;
        }
        public async Task SeedAppointmentAsync(IDbConnection connection,IDbTransaction transaction,int recordCount)
        {
            List<Appointment> appointments = new();
            try
            {
                var existingAppointments = await _appointmentRepository.GetExistingAppointments(connection,transaction);
                var appointmentStatusIds = await _appointmentStatusRepository.GetExistingAppointmentStatusIds(connection, transaction);
                var doctorIdsForAppointments = await _doctorRepository.GetExistingDoctorsIds(connection, transaction);
                var patientIdsForAppointments = await _patientRepository.GetExistingPatientIds(connection, transaction);

                while(recordCount > 0)
                {
                    var chunkSize = Math.Min(5000, recordCount);
                    for (int i = 0; i < chunkSize; i++)
                    {
                        appointments.Add(_appointmentGenerator.GenerateAppointment(doctorIdsForAppointments, patientIdsForAppointments, appointmentStatusIds, existingAppointments));
                    }
                        await _appointmentRepository.InsertAppointments(appointments, connection, transaction);
                    appointments.Clear();
                    recordCount -= chunkSize;
                }
                


                var appointmentsWithoutMedicalRecord = await _appointmentRepository.GetAppointmentIdsWithoutMedicalRecordAndWhenTheyWereCreated(connection, transaction);
                int processed = 0;
                while(processed < appointmentsWithoutMedicalRecord.Count)
                {
                    var chunkSize = Math.Min(5000, appointmentsWithoutMedicalRecord.Count);

                    var chunk = appointmentsWithoutMedicalRecord.Skip(processed).Take(chunkSize).ToList();
                    var medicalRecords = _medicalRecordGenerator.GenerateMedicalRecords(chunk);
                    await _medicalRecordRepository.InsertMedicalRecords(medicalRecords, connection, transaction);

                    processed += chunkSize;
                }

                var appointmentsWithoutPayment = await _appointmentRepository.GetAppointmentsWithoutPayment(connection, transaction);
                var paymentMethods = await _paymentMethodRepository.GetExistingPaymentMethodIds(connection, transaction);
                var paymentStatusIds = await _paymentStatusRepository.GetExistingPaymentStatusIds(connection, transaction);
                var usedPaymentNumber = await _paymentRepository.GetExistingPaymentNumbers(connection, transaction);

                processed = 0;
                while(processed < appointmentsWithoutPayment.Count)
                {
                    var chunkSize = Math.Min(5000, appointmentsWithoutPayment.Count - processed);
                    var chunk = appointmentsWithoutPayment.Skip(processed).Take(chunkSize).ToList();
                    var payments = _paymentGenerator.GeneratePayments(chunk, paymentMethods, paymentStatusIds, usedPaymentNumber);
                    await _paymentRepository.InsertPayments(payments, connection, transaction);
                    processed += chunkSize;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding Appointments: {ex.Message}");
                throw;
            }
        }
    }
}
