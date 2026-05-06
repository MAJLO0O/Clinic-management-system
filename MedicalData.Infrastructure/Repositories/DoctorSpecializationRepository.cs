using Dapper;
using MedicalData.Domain.Models;
using MedicalData.Infrastructure.DTOs;
using MedicalData.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class DoctorSpecializationRepository
    {
        public async Task<HashSet<(int, int)>> LoadExistingDoctorsSpecializations(IDbConnection connection, IDbTransaction transaction)
        {
            var result = new HashSet<(int, int)>();
            var sql = "select doctor_id,specialization_id from doctor_specialization";
            var rows = await connection.QueryAsync<(int doctorId, int specializationId)>(sql, transaction: transaction);
            foreach (var row in rows)
            {
                result.Add((row.doctorId, row.specializationId));
            }
            return result;

        }


        public async Task InsertDoctorSpecializations(IDbConnection connection, IDbTransaction transaction, List<DoctorSpecializationSnapshotDTO> doctorSpecializations)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            int i = 0;
            foreach (var doctorSpecialization in doctorSpecializations)
            {
                values.Add($"(@DoctorId{i}, @SpecializationId{i})");

                parameters.Add($"DoctorId{i}", doctorSpecialization.DoctorId);
                parameters.Add($"SpecializationId{i}", doctorSpecialization.SpecializationId);
                i++;
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync( sql.ToString(), parameters, transaction: transaction);
        }

        public async Task CreateDoctorSpecializations(IDbConnection connection, IDbTransaction transaction, int doctorId, List<int> specializationIds, CancellationToken ct)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
            var parameters = new DynamicParameters();
            var values = new List<string>();
            int i = 0;
            foreach (var specializationId in specializationIds)
            {
                values.Add($"(@DoctorId{i}, @SpecializationId{i})");

                parameters.Add($"DoctorId{i}", doctorId);
                parameters.Add($"SpecializationId{i}", specializationId);
                i++;
            }
            sql.Append(string.Join(",", values));
            await connection.ExecuteAsync(new CommandDefinition(sql.ToString(), parameters, transaction: transaction, cancellationToken: ct));
        }

        public async Task DeleteDoctorSpecializations(IDbConnection connection, IDbTransaction transaction, int doctorId, CancellationToken ct)
        {
            var sql = "delete from doctor_specialization where doctor_id = @DoctorId";
                await connection.ExecuteAsync(new CommandDefinition(sql, new { DoctorId = doctorId }, transaction: transaction, cancellationToken: ct));
        }



        public async Task WriteDoctorSpecializationJsonAsync(IDbConnection connection, Stream stream, CancellationToken ct)
        {
            var sql = "select doctor_id as DoctorId,specialization_id as SpecializationId from doctor_specialization";
            using var reader = await connection.ExecuteReaderAsync(sql);
            using var writer = new Utf8JsonWriter(stream);

            var doctorIdIndex = reader.GetOrdinal("DoctorId");
            var specializationIdIndex = reader.GetOrdinal("SpecializationId");
             
            writer.WriteStartArray();
                while (reader.Read())
                {
                    ct.ThrowIfCancellationRequested();  
                    var doctorSpecialization = MapToDoctorSpecialization(reader, doctorIdIndex, specializationIdIndex);
                    JsonSerializer.Serialize(writer, doctorSpecialization);
                }
                writer.WriteEndArray();
             
                await writer.FlushAsync();
            
        }





        public async Task ExportToJsonLocalAsync(IDbConnection connection, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_doctors_specializations.json");
            await using var stream = File.Create(path);

            await WriteDoctorSpecializationJsonAsync(connection, stream, ct);
        }
        public async Task AddToZipAsync(IDbConnection connection,ZipArchive zip, CancellationToken ct)
        {
            var entry = zip.CreateEntry("exported_doctors_specializations.json");
            await using var entryStream = entry.Open();
            await WriteDoctorSpecializationJsonAsync(connection, entryStream, ct);
        }


        public DoctorSpecializationSnapshotDTO MapToDoctorSpecialization(IDataReader reader, int doctorIdIndex, int specializationIdIndex)
        {
            return new DoctorSpecializationSnapshotDTO
            {
                DoctorId = reader.GetInt32(doctorIdIndex),
                SpecializationId = reader.GetInt32(specializationIdIndex)
            };
        }


        public async Task ImportFromJsonAsync(IDbConnection connection, IDbTransaction transaction, CancellationToken ct)
        {
            var path = Path.Combine(Path.GetTempPath(), "exported_doctors_specializations.json");
            using var stream = File.OpenRead(path);

            await ImportDoctorSpecializationAsync(connection, transaction, stream, ct);
        }
        public async Task ImportFromZipAsync(IDbConnection connection, IDbTransaction transaction, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.GetEntry("exported_doctors_specializations.json");
            if (entry == null)
                throw new InvalidOperationException("exported_doctors_specializations file is null");

            using var stream = entry.Open();
            await ImportDoctorSpecializationAsync(connection, transaction, stream, cancellationToken);

        }

        public async Task ImportDoctorSpecializationAsync(IDbConnection connection, IDbTransaction transaction, Stream stream, CancellationToken cancellationToken)
        {
            var sql = new StringBuilder();
            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
            var batch = new List<string>();
            var parameters = new DynamicParameters();
            try
            {
                await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<DoctorSpecializationSnapshotDTO>(stream, cancellationToken: cancellationToken))
                {
                    if (item != null)
                    {
                        int i = batch.Count;
                        batch.Add($"(@DoctorId{i}, @SpecializationId{i})");
                        parameters.Add($"DoctorId{i}", item.DoctorId);
                        parameters.Add($"SpecializationId{i}", item.SpecializationId);
                        if (batch.Count == 5000)
                        {
                            sql.Append(string.Join(",", batch));
                            await connection.ExecuteAsync(new CommandDefinition( sql.ToString(),parameters, transaction, cancellationToken: cancellationToken));
                            batch.Clear();
                            sql = new StringBuilder();
                            sql.Append("INSERT INTO doctor_specialization (doctor_id, specialization_id) VALUES ");
                            parameters = new DynamicParameters();
                        }
                    }
                }
                if(batch.Count > 0)
                {
                    sql.Append(string.Join(",", batch));
                    await connection.ExecuteAsync(new CommandDefinition( sql.ToString(),parameters, transaction, cancellationToken: cancellationToken));

                }
                Console.WriteLine("Imported doctor_specialization");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during import: {ex.Message}");
                throw;
            }
        }
    } 
}
