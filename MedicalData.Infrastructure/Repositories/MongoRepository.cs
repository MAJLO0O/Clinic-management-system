using MedicalData.Infrastructure.Helpers;
using MedicalData.Infrastructure.MongoModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.Repositories
{
    public class MongoRepository
    {
        private readonly IMongoCollection<AppointmentDocument> _appointmentCollection;
        public MongoRepository(IMongoClient client)
        {
            var mongoDatabase = client.GetDatabase("ClinicManagementSystem");
            _appointmentCollection = mongoDatabase.GetCollection<AppointmentDocument>("appointment");
        }
        public async Task InsertManyToMongoAsync(List<AppointmentDocument> appointmentDocuments)
        {
            await _appointmentCollection.InsertManyAsync(appointmentDocuments);
        }
        public async Task<int> GetMaxId()
        {
            var MaxId = await _appointmentCollection.Find(_=>true).SortByDescending(x=>x.Id).Limit(1).FirstOrDefaultAsync();
            return MaxId?.Id??0;
        }
        public async Task<PagedResult<AppointmentDocument>> GetMongoAppointment(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var filter = Builders<AppointmentDocument>.Filter.Gt(x => x.Id, lastId);
            var appointments = await _appointmentCollection.Find(filter).SortBy(x => x.Id).Limit(pageSize+1).ToListAsync(cancellationToken);
            
            return PaginationHelper.BuildPagedResult(appointments, pageSize, x => x.Id);
        }
        public async Task CleanMongoDb()
        {
            _appointmentCollection.DeleteMany(_ => true);
        }
    }
}
