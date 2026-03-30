using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Domain.Models
{
    public class Payment
    {
        public int PaymentNumber { get; set; }
        public decimal Amount { get; set; }
        public int AppointmentId { get; set; }
        public int? PaymentMethodId { get; set; }
        public int StatusId { get; set; }
    }
}
