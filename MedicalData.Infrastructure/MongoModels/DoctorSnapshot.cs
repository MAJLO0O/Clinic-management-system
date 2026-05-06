using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace MedicalData.Infrastructure.MongoModels
{
    public class DoctorSnapshot
    {
        [BsonElement("id")]
        public int Id { get; set; }
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        [BsonElement("lastName")]
        public string LastName { get; set; }
        [BsonElement("specializations")] 
        public List<string> Specializations { get; set; }
        [BsonElement("pesel")]
        public string Pesel { get; set; }
        [BsonElement("branchSnapshot")]
        public BranchSnapshot BranchSnapshot { get; set; }
    }
}
