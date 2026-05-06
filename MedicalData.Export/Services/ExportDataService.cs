using MedicalData.Infrastructure.Repositories;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using MedicalData.Export.Manifest;
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
        private readonly ExportManifestBuilder _manifestBuilder;
        public ExportDataService(IConfiguration configuration, ExportDataRepository exportDataRepository,
            AppointmentRepository appointmentRepository, AppointmentStatusRepository appointmentStatusRepository,
            BranchRepository branchRepository, DoctorRepository doctorRepository, DoctorSpecializationRepository doctorSpecializationRepository,
            MedicalRecordRepository medicalRecordRepository, PaymentMethodRepository paymentMethodRepository,
            PaymentRepository paymentRepository, PatientRepository patientRepository, SpecializationRepository specializationRepository, PaymentStatusRepository paymentStatusRepository, ExportManifestBuilder manifestBuilder)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find connection string");
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
            _manifestBuilder = manifestBuilder;
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
        public async Task ExportAppointmentsLocalAsync()
        {
            Console.WriteLine("starting export");
            using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            var ct = CancellationToken.None;
            await connection.OpenAsync();
            await _appointmentRepository.ExportToJsonLocalAsync(connection, ct);
            await _appointmentStatusRepository.ExportToJsonLocalAsync(connection, ct);
            await _branchRepository.ExportToJsonLocalAsync(connection,ct);
            await _doctorRepository.ExportToJsonLocalAsync(connection,ct);
            await _doctorSpecializationRepository.ExportToJsonLocalAsync(connection,ct);
            await _medicalRecordRepository.ExportToJsonLocalAsync(connection, ct);
            await _patientRepository.ExportToJsonLocalAsync(connection, ct);
            await _paymentMethodRepository.ExportToJsonLocalAsync(connection, ct);
            await _paymentRepository.ExportToJsonLocalAsync(connection, ct);
            await _paymentStatusRepository.ExportToJsonLocalAsync(connection, ct);
            await _specializationRepository.ExportToJsonLocalAsync(connection, ct);
            Console.WriteLine("export completed");
        }
        public async Task ExportZipAsync(Stream output, CancellationToken ct)
        {
            using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);
            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
            


            try
            {
                Console.WriteLine("export start");
                ct.ThrowIfCancellationRequested();
                await _appointmentRepository.AddAppointmentsToZipAsync(connection, zip, ct);
                await _appointmentStatusRepository.AddAppointmetStasusesToZipAsync(connection, zip, ct);
                await _branchRepository.AddBranchesToZipAsync(connection, zip, ct);
                await _doctorRepository.AddDoctorsToZipAsync(connection, zip, ct);
                await _doctorSpecializationRepository.AddToZipAsync(connection, zip, ct);
                await _medicalRecordRepository.AddToZipAsync(connection, zip, ct);
                await _patientRepository.AddToZipAsync(connection, zip, ct);
                await _paymentMethodRepository.AddToZipAsync(connection, zip, ct);
                await _paymentRepository.AddToZipAsync(connection, zip, ct);
                await _paymentStatusRepository.AddToZipAsync(connection, zip, ct);
                await _specializationRepository.AddToZipAsync(connection, zip, ct);
                await _manifestBuilder.AddToZipAsync(zip, ct);
                Console.WriteLine("export end");

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
