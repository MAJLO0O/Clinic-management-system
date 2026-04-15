using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class PaymentSnapshot
    {
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("number")] 
        public int Number { get; set; }
        [BsonElement("amount")]
        public decimal Amount { get; set; }
        [BsonElement("method")]
        public string Method { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
    }
}
