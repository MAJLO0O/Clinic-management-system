using MedicalData.Infrastructure.Mappers;
using MedicalData.Infrastructure.MongoModels;
using MedicalData.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.Services
{
    public class SyncService
    {
        private readonly string _connectionString;
        private readonly string _mongoConnectionString;
        private readonly IConfiguration _configuration;
        public SyncService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Postgres") ?? throw new Exception("Couldn't find postgreSql connection string");
            _mongoConnectionString = configuration.GetConnectionString("MongoDb") ?? throw new Exception("Couldn't find MongoDb connection string");
            _configuration = configuration;
        }
        public async Task SyncAsync()
        {
         var client = new MongoClient(_mongoConnectionString);
         var mongoImport = new MongoRepository(client);
         var exportRepository = new ExportDataRepository(_configuration);
            var mapper = new AppointmentMapper();
            var maxId = await mongoImport.GetMaxId();

            while(true)
            {

                var batchSize = 5000;
                var batch = await exportRepository.GetAboveId(batchSize, maxId);
                if (!batch.Any())
                    break;
                var mongoExportDocument = mapper.MapToAppointmentDocuments(batch);
                await mongoImport.InsertManyToMongoAsync(mongoExportDocument);
                maxId = batch.Max(x => x.AppointmentId);
            }
         
        }
       

    }
}
