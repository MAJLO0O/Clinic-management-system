using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using MedicalData.Infrastructure.MongoModels;
using MedicalData.Infrastructure.ReadDTOs;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MedicalData.Aplication.Services
{
    public class ReadService
    {
        private readonly string _connectionString;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly PatientRepository _patientRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly MongoRepository _mongoRepository;
        public ReadService(IConfiguration config, AppointmentRepository appointmentRepository, PatientRepository patientRepository, DoctorRepository doctorRepository, PaymentRepository paymentRepository, MongoRepository mongoRepository)
        {
            _connectionString = config.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find postgreSql connection string");
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _paymentRepository = paymentRepository;
            _mongoRepository = mongoRepository;
        }
        private async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken ct)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            return connection;
        }
        private int ValidatePageSize(int pageSize)
        {
            return Math.Clamp(pageSize, 1, 100);
        }
        public async Task<PagedResult<ReadAppointmentDTO>> GetAppointmentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            pageSize = ValidatePageSize(pageSize);

            return await _appointmentRepository.GetAppointmentsAsync(connection, lastId, pageSize, cancellationToken);
        }


        public async Task<PagedResult<ReadPatientDTO>> GetPatientsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            pageSize = ValidatePageSize(pageSize);

            return await _patientRepository.GetPatientsAsync(connection, lastId, pageSize, cancellationToken);
        }


        public async Task<PagedResult<ReadDoctorDTO>> GetDoctorsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            pageSize = ValidatePageSize(pageSize);

            return await _doctorRepository.GetDoctorsAsync(connection, lastId, pageSize, cancellationToken);
        }


        public async Task<PagedResult<ReadPaymentDTO>> GetPaymentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            using var connection = await CreateConnectionAsync(cancellationToken);
            pageSize = ValidatePageSize(pageSize);

            return await _paymentRepository.GetPaymentsAsync(connection, lastId, pageSize, cancellationToken);
        }


        public async Task<PagedResult<AppointmentDocument>> GetMongoAppointmentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            pageSize = ValidatePageSize(pageSize);
            return await _mongoRepository.GetMongoAppointment(lastId, pageSize, cancellationToken);
        }

    }
}
