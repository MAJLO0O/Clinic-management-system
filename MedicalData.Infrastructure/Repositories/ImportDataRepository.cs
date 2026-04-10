using Dapper;
using MedicalData.Infrastructure.MongoModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class ImportDataRepository
    {

        public async Task CleanAllData(IDbConnection connection)
        {
            var sql = @"TRUNCATE TABLE 
                        payment,
                        medical_record,
                        appointment,
                        doctor_specialization,
                        doctor,
                        patient,
                        branch,
                        appointment_status,
                        payment_method,
                        payment_status,
                        specialization
                    RESTART IDENTITY CASCADE; ";
            try
            {
                await connection.ExecuteAsync(sql);
                Console.WriteLine("Data Cleaned");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning data: {ex.Message}");
            }
        }

        
    }
}
