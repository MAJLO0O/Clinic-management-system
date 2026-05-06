using DataGenerator.Generators;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace DataGenerator.Services
{
    public class DoctorSeederService
    {
        private readonly string _connectionString;
        private readonly SpecializationRepository _specializationRepository;
        private readonly DoctorRepository _doctorRepository;
        private readonly DoctorSpecializationRepository _doctorSpecializationRepository;
        private readonly BranchRepository _branchRepository;
        private readonly DoctorGenereator _doctorGenerator;
        private readonly DoctorSpecializationGenerator _doctorSpecializationGenerator;

        public DoctorSeederService(IConfiguration configuration, SpecializationRepository specializationRepository, DoctorRepository doctorRepository, DoctorSpecializationRepository doctorSpecializationRepository, BranchRepository branchRepository, DoctorGenereator doctorGenerator, DoctorSpecializationGenerator doctorSpecializationGenerator)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Coulnd't find connection string");
            _specializationRepository = specializationRepository;
            _doctorRepository = doctorRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _branchRepository = branchRepository;
            _doctorGenerator = doctorGenerator;
            _doctorSpecializationGenerator = doctorSpecializationGenerator;
        }
        public async Task SeedDoctorsAsync(IDbConnection connection,IDbTransaction transaction,int recordCount)
        {
            List<Doctor> doctors = new List<Doctor>();
            try
            {
                var globalIndex = await _doctorRepository.CountDoctorsAsync(connection,transaction);
                var existingRelations = await _doctorSpecializationRepository.LoadExistingDoctorsSpecializations(connection, transaction);
                var branchIds = await _branchRepository.branchIds(connection,transaction);
                Console.WriteLine(recordCount);
                while(recordCount>0)
                {
                    
                    var batchSize = Math.Min(5000, recordCount);
                    for (int i = 0; i < batchSize; i++)
                    {
                        doctors.Add(_doctorGenerator.GenerateDoctor(branchIds, globalIndex));
                        globalIndex++;
                    }
                    await _doctorRepository.InsertDoctors(doctors, connection, transaction);
                    doctors.Clear();
                    recordCount -= batchSize;
                }
                var specializations = await _specializationRepository.GetExistingSpecializationIds(connection, transaction);
                var doctorIds = await _doctorRepository.GetExistingDoctorIdsWithoutSpecialization(connection, transaction);
                var newRelations = new List<DoctorSpecializationSnapshotDTO>();
                var doctorsCount = doctorIds.Count;
                int processsed = 0;
                while (processsed < doctorsCount)
                {
                    var doctorIdsBatch = doctorIds.Skip(processsed).Take(5000).ToList();
                    newRelations = _doctorSpecializationGenerator.GenerateDoctorSpecializationRelation(doctorIdsBatch, specializations, existingRelations);
                    Console.WriteLine($"New Relations: {newRelations.Count}");
                    await _doctorSpecializationRepository.InsertDoctorSpecializations(connection, transaction, newRelations);
                    processsed +=5000;
                }
                Console.WriteLine("Generated doctor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding doctors: {ex.Message}");
                throw;
            }
        }
    }
}
