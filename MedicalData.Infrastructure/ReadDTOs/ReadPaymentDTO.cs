using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.ReadDTOs
{
    public class ReadPaymentDTO
    {
        public int Id { get; set; }
        public int PaymentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime AppointmentStartingDateTime { get; set; }

        public string Method { get; set; }
        public string Status { get; set; }
        public string PatientFullName { get; set; }
        public string DoctorFullName { get; set; } 

    }
}
