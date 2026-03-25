using DataGenerator.Data;
using DataGenerator.Generators;
using DataGenerator.Models;
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
            try
            {
                var existingRelations = await _doctorSpecializationRepository.LoadExistingDoctorsSpecializations(connection, transaction);
                var branchIds = await _branchRepository.branchIds(connection,transaction);
                for (int i = 0; i < recordCount; i++)
                {
                    doctors.Add(_doctorGenerator.GenerateDoctor(branchIds));
                }
                await _doctorRepository.InsertDoctors(doctors, connection, transaction);
                var specializations = await _specializationRepository.GetExistingSpecializationIds(connection, transaction);
                var doctorIds = await _doctorRepository.GetExistingDoctorIdsWithoutSpecialization(connection, transaction);
                var newRelations = _doctorSpecializationGenerator.GenerateDoctorSpecializationRelation(doctorIds, specializations, existingRelations);
                await _doctorSpecializationRepository.InsertDoctorSpecializations(newRelations, connection, transaction);
                await transaction.CommitAsync();
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
