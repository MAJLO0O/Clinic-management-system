using Microsoft.Extensions.Configuration;
using MedicalData.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedicalData.Infrastructure.DTOs;
using System.Data;

namespace MedicalData.Aplication.Services.CRUD
{
    public class SpecializationService
    {
        private readonly string _connectionString;
        private readonly SpecializationRepository _specializationRepository;

        public SpecializationService(IConfiguration configuration, SpecializationRepository specializationRepository)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Couldn't find connection string");
            _specializationRepository = specializationRepository;
        }
        public async Task<List<SpecializationSnapshotDTO>> GetSpecializationsAsync(CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);

            try
            {
                var result = await _specializationRepository.GetSpecializationsAsync(connection, ct);
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error getting specializations {ex.Message}");
                throw;
            }
        }
    }
}
