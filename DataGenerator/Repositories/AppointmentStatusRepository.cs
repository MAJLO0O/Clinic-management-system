using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Data
{
    public class AppointmentStatusRepository
    {   
        public async Task<List<int>> GetExistingAppointmentStatusIds(IDbConnection connection,IDbTransaction transaction)
        {
                var sql = "select id from appointment_status";
                var statusIds = await connection.QueryAsync<int>(sql, transaction: transaction);
                return statusIds.ToList();
            }
        }
    }

