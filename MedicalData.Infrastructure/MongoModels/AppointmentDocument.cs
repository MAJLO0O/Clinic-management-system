using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class AppointmentDocument
    {
        //to-do implemet maper model
        public int Id { get; set; }
        public DateTime StartingDateTime { get; set; }
        public string Status { get; set; }


       public MedicalRecordSnapshot? MedicalRecordSnapshot { get; set; }

        public DoctorSnapshot DoctorSnapshot { get; set; }
        public PatientSnapshot PatientSnapshot { get; set; }
        public PaymentSnapshot? PaymentSnapshot { get; set; }
    }
}
