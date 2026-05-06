using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.Services.CRUD
{
    public class BranchService
    {
        private readonly string _connectionString;
        private readonly BranchRepository _branchRepository;
        public BranchService(IConfiguration config, BranchRepository branchRepository)
        {
            _connectionString = config.GetConnectionString("Postgres") ?? throw new ArgumentNullException("Couldn't find connection string");
            _branchRepository = branchRepository;
        }
        public async Task<List<BranchSnapshotDTO>> GetBranchesAsync(CancellationToken ct)
        {
            await using var connection = DbConnectionFactory.CreateDbConnection(_connectionString);
            await connection.OpenAsync(ct);
            try
            {
                var result = await _branchRepository.GetBranchesAsync(connection, ct);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting branches {ex.Message}");
                throw;
            }
        }
    }
}
