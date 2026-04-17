using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class AppointmentDocument
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("startingDateTime")]
        public DateTime StartingDateTime { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("medicalRecordSnapshot")]
       public MedicalRecordSnapshot? MedicalRecordSnapshot { get; set; }
        
        [BsonElement("doctorSnapshot")]
        public DoctorSnapshot DoctorSnapshot { get; set; }
        [BsonElement("patientSnapshot")]
        public PatientSnapshot PatientSnapshot { get; set; }
        [BsonElement("paymentSnapshot")]
        public PaymentSnapshot? PaymentSnapshot { get; set; }
    }
}
