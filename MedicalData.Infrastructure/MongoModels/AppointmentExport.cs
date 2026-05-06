using MedicalData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.NoSqlDTOs
{
    public class AppointmentExport
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentStartingDateTime { get; set; }
        public string AppointmentStatus { get; set; }


        public string MedicalRecordNote { get; set; }
        public DateTime MedicalRecordCreatedAt { get; set; }


        public int PaymentId { get; set; }
        public int PaymentNumber { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }


        public int DoctorId { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string[] DoctorSpecializations { get; set; }
        public string DoctorPesel { get; set; }


        public int DoctorBranchId { get; set; }
        public string DoctorBranchCity { get; set; }
        public string DoctorBranchAddress { get; set; }

        public int PatientId { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public string PatientPesel { get; set; }

    }
}
