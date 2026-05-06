using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using MedicalData.Import.ZipReader;
using Npgsql;
using System.Diagnostics;
using MongoDB.Driver;
using MedicalData.Infrastructure.Mappers;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;

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
        private readonly ManifestReader _manifestReader;
        private readonly MongoRepository _mongoRepository;

        public MedicalDataImportService(IConfiguration config, DoctorRepository doctorRepository, PatientRepository patientRepository, AppointmentRepository appointmentRepository,
            ImportDataRepository importDataRepository, AppointmentStatusRepository appointmentStatusRepository, BranchRepository branchRepository,
            DoctorSpecializationRepository doctorSpecializationRepository, SpecializationRepository specializationRepository, PaymentMethodRepository paymentMethodRepository,
            PaymentStatusRepository paymentStatusRepository, MedicalRecordRepository medicalRecordRepository, PaymentRepository paymentRepository, ManifestReader manifestReader, MongoRepository mongoRepository)
        {
            _connectionString = config.GetConnectionString("Postgres") ?? throw new InvalidDataException("Couldn't find connection string");
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
            _manifestReader = manifestReader;
            _mongoRepository = mongoRepository;
        }
        public async Task ImportDataFromJson(CancellationToken ct)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Cleaning data...");
            await _importDataRepository.CleanAllDataAsync(connection, transaction, ct);
            await _appointmentStatusRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _branchRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _specializationRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _paymentMethodRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _paymentStatusRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _patientRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _doctorRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _doctorSpecializationRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _appointmentRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _medicalRecordRepository.ImportFromJsonAsync(connection, transaction, ct);
            await _paymentRepository.ImportFromJsonAsync(connection, transaction, ct);
            stopwatch.Stop();
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds / 1000} s");
        }

        public async Task ImportDataFromZipAsync(Stream input, CancellationToken ct)
        {
            using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
            var entities = await _manifestReader.ReadManifestAsync(zip, ct);
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            using var transaction = connection.BeginTransaction();
            var handler = new Dictionary<string, Func<Task>>()
            {
                ["appointment"] = () => _appointmentRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["appointment_status"] = () => _appointmentStatusRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["branch"] = () => _branchRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["doctor"] = () => _doctorRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["doctor_specialization"] = () => _doctorSpecializationRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["medical_record"] = () => _medicalRecordRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["patient"] = () => _patientRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["payment_method"] = () => _paymentMethodRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["payment_status"] = () => _paymentStatusRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["payment"] = () => _paymentRepository.ImportFromZipAsync(connection, transaction, zip, ct),
                ["specialization"] = () => _specializationRepository.ImportFromZipAsync(connection, transaction, zip, ct)
            };
            var orderedEntities = entities.Entities.OrderBy(e => e.Order);
            try
            {
                await _importDataRepository.CleanAllDataAsync(connection, transaction, ct);
                foreach (var entity in orderedEntities)
                {
                    if (handler.TryGetValue(entity.EntityName, out var importFunc))
                    {
                        Console.WriteLine($"Importing {entity.EntityName}...");
                        await importFunc();
                    }
                    else
                    {
                        throw new InvalidOperationException($"No handler for entity {entity.EntityName}");
                    }
                }
                Console.WriteLine("Import DONE!!!");
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data: {ex.Message}");
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task CleanDataBaseAsync(CancellationToken ct)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            using var transaction = await connection.BeginTransactionAsync(ct);
            try
            {
                Console.WriteLine("Cleaning data...");
                await _importDataRepository.CleanMainTablesAsync(connection, transaction, ct);
                await _mongoRepository.CleanMongoDb();
                Console.WriteLine("Done!!!");
                await transaction.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning data: {ex.Message}");
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
