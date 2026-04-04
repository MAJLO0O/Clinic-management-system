using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using DataGenerator.Data;
namespace MedicalData.Export.Services

{
    public class ExportDataService
    {
        private readonly string _connectionString;
        private readonly ExportDataRepository _exportDataRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly AppointmentStatusRepository _appointmentStatusRepository;
        private readonly BranchRepository _branchRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly DoctorSpecializationRepository _doctorSpecializationRepository;
        private readonly MedicalRecordRepository _medicalRecordRepository;
        private readonly PaymentMethodRepository _paymentMethodRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly PaymentStatusRepository _paymentStatusRepository;
        private readonly PatientRepository _patientRepository;
        private readonly SpecializationRepository _specializationRepository;
        public ExportDataService(string connectionString, ExportDataRepository exportDataRepository,
            AppointmentRepository appointmentRepository, AppointmentStatusRepository appointmentStatusRepository,
            BranchRepository branchRepository, DoctorRepository doctorRepository, DoctorSpecializationRepository doctorSpecializationRepository,
            MedicalRecordRepository medicalRecordRepository, PaymentMethodRepository paymentMethodRepository,
            PaymentRepository paymentRepository, PatientRepository patientRepository, SpecializationRepository specializationRepository, PaymentStatusRepository paymentStatusRepository)
        {
            _connectionString = connectionString;
            _exportDataRepository = exportDataRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentStatusRepository = appointmentStatusRepository;
            _branchRepository = branchRepository;
            _doctorRepository = doctorRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentRepository = paymentRepository;
            _patientRepository = patientRepository;
            _specializationRepository = specializationRepository;
            _paymentStatusRepository = paymentStatusRepository;
        }

        public async Task ExportDataToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var appointments = await _exportDataRepository.GetExportDataAsync();
            var jsonString = JsonSerializer.Serialize(appointments, options);
            await File.WriteAllTextAsync("exported_appointments.json", jsonString);
        }
        public async Task ExportAppointmentsAsync()
        {
            Console.WriteLine("starting export");
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await _appointmentRepository.ExportAllDataFromAppointmet(connection);
            await _appointmentStatusRepository.ExportAppointmentsStatusesAsync(connection);
            await _branchRepository.ExportBranchesAsync(connection);
            await _doctorRepository.ExportDoctorsAsync(connection);
            await _doctorSpecializationRepository.ExportDoctorSpecializationAsync(connection);
            await _medicalRecordRepository.ExportMedicalRecordAsync(connection);
            await _patientRepository.ExportPatientsAsync(connection);
            await _paymentMethodRepository.ExportPaymentMethodsAsync(connection);
            await _paymentRepository.ExportPaymentsAsync(connection);
            await _paymentStatusRepository.ExportPaymentStatusesAsync(connection);
            await _patientRepository.ExportPatientsAsync(connection);
            await _specializationRepository.ExportSpecializationsAsync(connection);
            Console.WriteLine("export completed");
        }

    }
}
