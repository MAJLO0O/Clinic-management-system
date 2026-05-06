using DataGenerator.DTO;
using DataGenerator.Services;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver.Core.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.Services
{
    public class BenchmarkService
    {
        private readonly string _connectionString;
        private readonly IndexBenchmarkService _indexBenchmarkService;
        private readonly SyncService _syncService;
        private readonly ImportDataRepository _importDataRepository;
        private readonly MongoRepository _mongoRepository;
        private readonly DoctorSeederService _doctorSeederService;
        private readonly AppointmentDataSeeder _appointmentDataSeeder;
        private readonly PatientDataSeeder _patientDataSeeder;

        public BenchmarkService(IConfiguration config, IndexBenchmarkService indexBenchmarkService, SyncService syncService
            , ImportDataRepository importDataRepository, MongoRepository mongoRepository, DoctorSeederService doctorSeederService, AppointmentDataSeeder appointmentDataSeeder, PatientDataSeeder patientDataSeeder)
        {
            _indexBenchmarkService = indexBenchmarkService;
            _syncService = syncService;
            _importDataRepository = importDataRepository;
            _mongoRepository = mongoRepository;
            _doctorSeederService = doctorSeederService;
            _appointmentDataSeeder = appointmentDataSeeder;
            _patientDataSeeder = patientDataSeeder;
            _connectionString = config.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find postgreSql connection string");
        }
        public async Task<BenchmarkResultDTO> BenchmarkAllAsync(int recordCount, CancellationToken ct)
        {
            
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            using var transaction = await connection.BeginTransactionAsync(ct);
            try
            {
                await _importDataRepository.CleanMainTablesAsync(connection, transaction, ct);
                await _mongoRepository.CleanMongoDb();

                await _doctorSeederService.SeedDoctorsAsync(connection, transaction, recordCount);
                await _patientDataSeeder.SeedPatientsAsync(connection, transaction, 2 * recordCount);
                await _appointmentDataSeeder.SeedAppointmentAsync(connection, transaction, 5 * recordCount);
                await transaction.CommitAsync(ct);


                await _syncService.SyncAsync();

                var totalCount = (1+2+5) * recordCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during data seeding: {ex.Message}");
                await transaction.RollbackAsync(ct);
                throw;
            }

            Console.WriteLine("Benchmarking SQL with indexes...");
            double sqlWithIndexesTime = await _indexBenchmarkService.BenchmarkWithIndexes();
            Console.WriteLine("Benchmarking SQL without indexes...");
            double sqlWithoutIndexesTime = await _indexBenchmarkService.BenchmarkWithoutIndexes();
            Console.WriteLine("Benchmarking NoSQL with indexes...");
            double noSqlWithIndexesTime = await _indexBenchmarkService.BenchmarkMongoWithIndexes();
            Console.WriteLine("Benchmarking NoSQL without indexes...");
            double noSqlWithoutIndexesTime = await _indexBenchmarkService.BenchmarkMongoWithoutIndexes();
            Console.WriteLine("\nSummary:");
            Console.WriteLine($"Average SQL time with indexes: {sqlWithIndexesTime} ms");
            Console.WriteLine($"Average SQL time without indexes: {sqlWithoutIndexesTime} ms");
            Console.WriteLine($"Average NoSQL time with indexes: {noSqlWithIndexesTime} ms");
            Console.WriteLine($"Average NoSQL time without indexes: {noSqlWithoutIndexesTime} ms");
            return new BenchmarkResultDTO
            {
                SqlWithIndexesAvgMs = sqlWithIndexesTime,
                SqlWithoutIndexesAvgMs = sqlWithoutIndexesTime,
                NoSqlWithIndexesAvgMs = noSqlWithIndexesTime,
                NoSqlWithoutIndexesAvgMs = noSqlWithoutIndexesTime,
                TotalRecords = recordCount
            };
        }
    }
}
