using MedicalData.Domain.Models;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.Services.CRUD
{
    public class PatientService
    {
        private readonly string _connectionString;
        private readonly PatientRepository _patientRepository;
        public PatientService(IConfiguration configuration, PatientRepository patientRepository)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Couldn't find connection string");
            _patientRepository = patientRepository;
        }
        public async Task CreatePatientAsync(Patient patient, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await _patientRepository.CreatePatientAsync(patient, connection, transaction, ct);
                await transaction.CommitAsync(ct);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        public async Task<bool> UpdatePatientAsync(int id, Patient patient, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var result = await _patientRepository.UpdatePatientAsync(id, patient, connection, transaction, ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

        }
        public async Task<bool> DeletePatientAsync(int patientId, CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var result = await _patientRepository.DeletePatientAsync(patientId, connection, transaction, ct);
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
