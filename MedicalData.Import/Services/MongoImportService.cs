using MedicalData.Infrastructure.Mappers;
using MedicalData.Infrastructure.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Import.Services
{
    public class MongoImportService
    {
        private readonly string _mongoConnectionString;
        private readonly AppointmentMapper _appointmentMapper;
        private readonly ExportDataRepository _exportDataRepository;

        public MongoImportService(string mongoConnectionString, AppointmentMapper appointmentMapper, ExportDataRepository exportDataRepository)
        {
            _mongoConnectionString = mongoConnectionString;
            _appointmentMapper = appointmentMapper;
            _exportDataRepository = exportDataRepository;
        }
        public async Task ImportDataToMongoDB()
        {
            using var mongoClient = new MongoClient(_mongoConnectionString);
            var mongoImportRepository = new MongoRepository(mongoClient);
            var offset = 0;
            var chunk = 10000;
            while (true) 
            {
                var batch = await _exportDataRepository.ExportAppointmentToNoSqlAsync(offset,chunk);
                if (!batch.Any())
                    break;
                var appointmentDocuments = _appointmentMapper.MapToAppointmentDocuments(batch);
                await mongoImportRepository.InsertManyToMongoAsync(appointmentDocuments);
                offset += chunk;
            }
            Console.WriteLine($"Processed {offset} records");
        }
    }
}
