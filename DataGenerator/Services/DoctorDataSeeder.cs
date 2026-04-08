using DataGenerator.Data;
using DataGenerator.Generators;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Repositories;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DoctorSeederService(string connectionString, SpecializationRepository specializationRepository, DoctorRepository doctorRepository, DoctorSpecializationRepository doctorSpecializationRepository, BranchRepository branchRepository, DoctorGenereator doctorGenerator, DoctorSpecializationGenerator doctorSpecializationGenerator)
        {
            _connectionString = connectionString;
            _specializationRepository = specializationRepository;
            _doctorRepository = doctorRepository;
            _doctorSpecializationRepository = doctorSpecializationRepository;
            _branchRepository = branchRepository;
            _doctorGenerator = doctorGenerator;
            _doctorSpecializationGenerator = doctorSpecializationGenerator;
        }
        public async Task SeedDoctorsAsync(int recordCount)
        {
            List<Doctor> doctors = new List<Doctor>();
            using NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            int globalIndex = 0;
            try
            {
                var existingRelations = await _doctorSpecializationRepository.LoadExistingDoctorsSpecializations(connection, transaction);
                var branchIds = await _branchRepository.branchIds(connection,transaction);
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
                    await _doctorSpecializationRepository.InsertDoctorSpecializations(newRelations, connection, transaction);
                    processsed +=5000;
                }
                await transaction.CommitAsync();
                Console.WriteLine("Generated doctor");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error seeding doctors: {ex.Message}");
                throw;

            }
        }
    }
}
