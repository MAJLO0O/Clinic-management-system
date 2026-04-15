using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using DataGenerator.Data;
using Dapper;
using Npgsql;
using System.Data;
using System.Diagnostics;
using MongoDB;
using MongoDB.Driver;
using MedicalData.Infrastructure.MongoModels;
using MongoDB.Bson;

namespace DataGenerator.Services
{
    public class IndexBenchmarkService
    {
        private readonly string _connectionString;
        private readonly IMongoCollection<AppointmentDocument> _collection;
        public IndexBenchmarkService(string connectionString, MongoClient client)
        {
            _connectionString = connectionString;
            var mongoDatabase = client.GetDatabase("ClinicManagementSystem");
            _collection = mongoDatabase.GetCollection<AppointmentDocument>("appointment");    
        } 
        public async Task CreateIndexAsync()
        {
           using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = "create index if not exists idx_appointment_starting_time on appointment (starting_date_time);";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_appointment_starting_date on appointment (date(starting_date_time));";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_ds_doctor_specialization on doctor_specialization (doctor_id,specialization_id);";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_payment_appointment on payment(appointment_id);";
            await connection.ExecuteAsync(sql);

        }
        public async Task DropIndexAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = "DROP INDEX if exists idx_appointment_starting_time;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_appointment_starting_date;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_ds_doctor_specialization;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_payment_appointment;";
            await connection.ExecuteAsync(sql);
        }
        
        public async Task RunQueriesAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var sql = "select appointment_id from payment p inner join appointment a on p.appointment_id=a.id where p.appointment_id>10 and starting_date_time<'2026-01-01 00:00:00';";
            await connection.QueryAsync<int>(sql);

            sql = "UPDATE appointment SET starting_date_time = starting_date_time + interval '1 day' WHERE starting_date_time > '2025-10-08 00:00:00' and starting_date_time < '2025-10-09 00:00:00';";
            await connection.ExecuteAsync(sql);
            
            sql = "select doctor_id,specialization_id from doctor_specialization where doctor_id=10 and specialization_id between 8 and 20";
            await connection.QueryAsync<(int doctorId, int specializationId)>(sql);

        }

        public async Task RunNoSqlQueriesAsync()
        {
            
            var filter = Builders<AppointmentDocument>.Filter.And(
                Builders<AppointmentDocument>.Filter.Gt(a => a.Id, 10),
                Builders<AppointmentDocument>.Filter.Lt(a => a.StartingDateTime, new DateTime(2026, 1, 1))
            );
            var appointment = await _collection.Find(filter).ToListAsync();


            var filterUpdate = Builders<AppointmentDocument>.Filter.And(
                Builders<AppointmentDocument>.Filter.Gt(a => a.StartingDateTime, new DateTime(2025, 10, 8)),
                Builders<AppointmentDocument>.Filter.Lt(a => a.StartingDateTime, new DateTime(2025, 10, 9))
            );

            var pipeline = new[]
            {
                new BsonDocument("$set", new BsonDocument
                {
                    { "StartingDateTime", new BsonDocument
                    {
                        {  "$dateAdd", new BsonDocument
                        {
                            {"startDate","$StartingDateTime" },
                            {"amount", 1},
                            {"unit", "day"}
                        }
                        }
                    }
                    }
                })};

            var update = new PipelineUpdateDefinition<AppointmentDocument>(pipeline);
            await _collection.UpdateManyAsync(filter, update);

            filter = Builders<AppointmentDocument>.Filter.And(
                Builders<AppointmentDocument>.Filter.Eq(x=> x.DoctorSnapshot.Id,10),
                Builders<AppointmentDocument>.Filter.AnyIn(
                    x=>x.DoctorSnapshot.Specializations,
                    new [] {"Cariology", "Gynecology", "Dermatology"}));

            var specializations = _collection.Find(filter);
        }

        public async Task CreateMongoIndexesAsync()
        {
            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<AppointmentDocument>(
                    Builders<AppointmentDocument>.IndexKeys
                    .Ascending(a=>a.StartingDateTime)
                    )
                );

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<AppointmentDocument>(
                    Builders<AppointmentDocument>.IndexKeys
                    .Ascending(x=>x.DoctorSnapshot.Id)
                    .Ascending(x=>x.DoctorSnapshot.Specializations)
                    )
                );

        }

        public async Task DropMongoIndexesAsync()
        {
            await _collection.Indexes.DropAllAsync();
        }


        public async Task BenchmarkWithIndexes()
        {
            await CreateIndexAsync();
            await RunQueriesAsync();
            List<double> times = new();
            for(int i = 0; i < 5; i++)
            {
            var stopwatch = Stopwatch.StartNew();
            await RunQueriesAsync();
            stopwatch.Stop();
            Console.WriteLine($"Time to complete SQL with indexes {stopwatch.Elapsed.TotalMilliseconds} ms");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average SQL time with indexes is {times.Average()} ms");
        }
        public async Task BenchmarkWithoutIndexes()
        {
            using NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            await DropIndexAsync();
            await RunQueriesAsync();
            List<double> times = new();
            for(int i=0; i<5;i++)
            {
                var stopwatch = Stopwatch.StartNew();
                await RunQueriesAsync();
                stopwatch.Stop();
                Console.WriteLine($"Time to complete SQL without indexes {stopwatch.Elapsed.TotalMilliseconds} ms");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
                Console.WriteLine($"Average SQL time without indexes is {times.Average()} ms");
        }
        public async Task BenchmarkMongoWithoutIndexes()
        {
            Stopwatch stopwatch = new();
            await DropMongoIndexesAsync();
            await RunNoSqlQueriesAsync();
            List<double> times = new();
            for (int i =0;i<5;i++)
            {
                stopwatch.Start();
                await RunNoSqlQueriesAsync();
                stopwatch.Stop();
                Console.WriteLine($"Time to complete NoSQL without indexes: {stopwatch.Elapsed.TotalMilliseconds}ms");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average NoSQL time without indexes is {times.Average()}ms");
        }
        public async Task BenchmarkMongoWithIndexes()
        {
            Stopwatch stopwatch = new();
            await CreateMongoIndexesAsync();
            await RunNoSqlQueriesAsync();
            List<double> times = new();
            for (int i=0;  i<5;i++)
            {
                stopwatch.Start();
                await RunNoSqlQueriesAsync();
                stopwatch.Stop();
                Console.WriteLine($"Time to complete NoSql with Indexes: {stopwatch.Elapsed.TotalMilliseconds}ms ");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average NoSql time with Indexes is {times.Average()}ms");
        }
    }
       
        
}

