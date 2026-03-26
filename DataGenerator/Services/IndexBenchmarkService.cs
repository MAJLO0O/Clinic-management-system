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
namespace DataGenerator.Services
{
    public class IndexBenchmarkService
    {
        private readonly string _connectionString;

        public IndexBenchmarkService(string connectionString)
        {
            _connectionString = connectionString;
        } 
        public async Task CreateIndexAsync(IDbConnection connection)
        {
            var sql = "create index if not exists idx_appointment_starting_time on appointment (starting_date_time);";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_appointment_starting_date on appointment (date(starting_date_time));";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_ds_doctor_specialization on doctor_specialization (doctor_id,specialization_id);";
            await connection.ExecuteAsync(sql);

            sql = "create index if not exists idx_payment_appointment on payment(appointment_id);";
            await connection.ExecuteAsync(sql);

        }
        public async Task DropIndexAsync(IDbConnection connection)
        {
            var sql = "DROP INDEX if exists idx_appointment_starting_time;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_appointment_starting_date;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_ds_doctor_specialization;";
            await connection.ExecuteAsync(sql);

            sql = "DROP INDEX if exists idx_payment_appointment;";
            await connection.ExecuteAsync(sql);
        }
        
        public async Task RunQueriesAsync(IDbConnection connection)
        {
            var sql = "select appointment_id from payment p inner join appointment a on p.appointment_id=a.id where p.appointment_id>10 and starting_date_time<NOW();";
            await connection.QueryAsync<int>(sql);

            sql = "select id,starting_date_time from appointment where date(starting_date_time)='2022-10-08';";
            await connection.QueryAsync<(int Id,DateTime Date)>(sql);
            
            sql = "select doctor_id,specialization_id from doctor_specialization where doctor_id=10 and specialization_id=12";
            await connection.QueryAsync<(int doctorId, int specializationId)>(sql);

        }
        public async Task BenchmarkWithIndexes()
        {
            using NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            await CreateIndexAsync(connection);
            await RunQueriesAsync(connection);
            List<double> times = new();
            for(int i = 0; i < 5; i++)
            {
            var stopwatch = Stopwatch.StartNew();
            await RunQueriesAsync(connection);
            stopwatch.Stop();
            Console.WriteLine($"Time to complete with indexes {stopwatch.Elapsed.TotalMilliseconds} ms");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
            Console.WriteLine($"Average time with indexes is {times.Average()} ms");
        }
        public async Task BenchmarkWithoutIndexes()
        {
            using NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            await DropIndexAsync(connection);
            await RunQueriesAsync(connection);
            List<double> times = new();
            for(int i=0; i<5;i++)
            {
                var stopwatch = Stopwatch.StartNew();
                await RunQueriesAsync(connection);
                stopwatch.Stop();
                Console.WriteLine($"Time to complete without indexes {stopwatch.Elapsed.TotalMilliseconds} ms");
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
                Console.WriteLine($"Average time without indexes is {times.Average()} ms");
        }
        
    }
       
        
}

