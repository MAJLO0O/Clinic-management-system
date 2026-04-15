using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace MedicalData.Infrastructure.MongoModels
{
    public class PatientSnapshot
    {
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        [BsonElement("lastName")]
        public string LastName { get; set; }
        [BsonElement("pesel")]
        public string Pesel { get; set; }
    }
}
