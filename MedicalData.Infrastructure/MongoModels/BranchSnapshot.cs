using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class BranchSnapshot
    {
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("city")]
        public string City { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
    }
}
