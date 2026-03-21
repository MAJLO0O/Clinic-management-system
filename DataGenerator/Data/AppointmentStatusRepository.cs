using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class AppointmentStatusRepository
    {   
        private readonly string _connectionString;
        public AppointmentStatusRepository(string connectionString) 
        {
            _connectionString = connectionString;
        }
        public async Task<List<int>> GetExistingAppointmentStatusIds()
        {
            using (var connection = DbConnectionFactory.CreateDbConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "select id from appointment_status";
                var statusIds = await connection.QueryAsync<int>(sql);
                return statusIds.ToList();
            }
        }
    }
}
