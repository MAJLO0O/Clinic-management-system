using MedicalData.Infrastructure.MongoModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class MongoImportRepository
    {
        private readonly IMongoCollection<AppointmentDocument> _appointmentCollection;
        public MongoImportRepository(MongoClient client)
        {
            var mongoDatabase = client.GetDatabase("ClinicManagementSystem");
            _appointmentCollection = mongoDatabase.GetCollection<AppointmentDocument>("appointment");
        }
        public async Task InsertManyToMongoAsync(List<AppointmentDocument> appointmentDocuments)
        {
            await _appointmentCollection.InsertManyAsync(appointmentDocuments);
        }
    }
}
