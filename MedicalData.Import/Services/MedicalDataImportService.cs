using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using Npgsql;
using DataGenerator.Data;
using System.Diagnostics;

namespace MedicalData.Import.Services
{
    public class MedicalDataImportService
    {
        private readonly string _connectionString;
        private readonly DoctorRepository _doctorRepository;
        private readonly PatientRepository _patientRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly ImportDataRepository _importDataRepository;
        private readonly AppointmentStatusRepository _appointmentStatusRepository;
        private readonly BranchRepository _branchRepository;
        private readonly DoctorSpecializationRepository _doctorSpecializationRepository;
        private readonly SpecializationRepository _specializationRepository;
        private readonly PaymentMethodRepository _paymentMethodRepository;
        private readonly PaymentStatusRepository _paymentStatusRepository;
        private readonly MedicalRecordRepository _medicalRecordRepository;
        private readonly PaymentRepository _paymentRepository;
        public MedicalDataImportService(string connectionString,DoctorRepository doctorRepository, PatientRepository patientRepository, AppointmentRepository appointmentRepository,
            ImportDataRepository importDataRepository, AppointmentStatusRepository appointmentStatusRepository,BranchRepository branchRepository,
            DoctorSpecializationRepository doctorSpecializationRepository,SpecializationRepository specializationRepository, PaymentMethodRepository paymentMethodRepository,
            PaymentStatusRepository paymentStatusRepository, MedicalRecordRepository medicalRecordRepository, PaymentRepository paymentRepository)
        {
            _connectionString = connectionString;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
            _importDataRepository = importDataRepository;
            _appointmentStatusRepository = appointmentStatusRepository;
            _branchRepository = branchRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _specializationRepository = specializationRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentStatusRepository = paymentStatusRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task ImportDataFromJson()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Cleaning data...");
            await _importDataRepository.CleanAllData(connection);
            await _appointmentStatusRepository.InsertAppointmentStatusAsync(connection);
            await _branchRepository.ImportBranchesAsync(connection);
            await _specializationRepository.ImportSpecializationAsync(connection);
            await _paymentMethodRepository.ImportPaymentMethodAsync(connection);
            await _paymentStatusRepository.ImportPaymentStatusAsync(connection);
            await _patientRepository.ImportPatientsAsync(connection);
            await _doctorRepository.ImportDoctorsAsync(connection);
            await _doctorSpecializationRepository.ImportDoctorSpecializationAsync(connection);
            await _appointmentRepository.ImportAppointmentAsync(connection);
            await _medicalRecordRepository.ImportMedicalRecordsAsync(connection);
            await _paymentRepository.ImportPaymentAsync(connection);
            stopwatch.Stop();
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds/1000} s");
        }
    }
}
