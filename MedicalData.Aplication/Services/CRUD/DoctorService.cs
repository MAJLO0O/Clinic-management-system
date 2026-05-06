using Microsoft.Extensions.Configuration;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using MedicalData.Infrastructure.DTOs;
namespace MedicalData.Aplication.Services.CRUD
{
    public class DoctorService
    {
        private readonly string _connectionString;
        private readonly DoctorRepository _doctorRepository;
        private readonly DoctorSpecializationRepository _doctorSpecializationRepository;

        public DoctorService(IConfiguration configuration, DoctorRepository doctorRepository, DoctorSpecializationRepository doctorSpecializationRepository)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Coulnd't find connection string");
            _doctorRepository = doctorRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
        }
        public async Task CreateDoctorAsync(Doctor doctor, List<int> specializationIds, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);

            try
            {
                var doctorId = await _doctorRepository.InsertDoctorAsync(connection, transaction, doctor, ct);
                if (specializationIds != null && specializationIds.Count > 0)
                {
                    await _doctorSpecializationRepository.CreateDoctorSpecializations(connection, transaction, doctorId, specializationIds, ct);  
                }
                await transaction.CommitAsync(ct);
                
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<bool> UpdateDoctorAsync(int id, Doctor doctor, List<int> specializationIds, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);
            try
            {
                var result = await _doctorRepository.UpdateDoctorAsync(id, doctor, connection, transaction, ct);
                await _doctorSpecializationRepository.DeleteDoctorSpecializations(connection, transaction, id, ct);
                if (specializationIds != null &&  specializationIds.Count > 0) 
                {
                    await _doctorSpecializationRepository.CreateDoctorSpecializations(connection, transaction, id, specializationIds, ct);
                }
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<bool> DeleteDoctorAsync(int doctorId, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);
            try
            {
                var result = await _doctorRepository.SoftDeleteDoctorAsync(connection, transaction,doctorId, ct);
                await _doctorSpecializationRepository.DeleteDoctorSpecializations(connection, transaction, doctorId, ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
