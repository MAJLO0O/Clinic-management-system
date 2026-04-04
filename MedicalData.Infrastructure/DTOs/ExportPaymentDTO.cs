using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.DTOs
{
    public class ExportPaymentDTO
    {
        public int Id { get; set; }
        public int PaymentNumber { get; set; }
        public decimal Amount { get; set; }
        public int PaymentStatusId { get; set; }
        public int AppointmentId { get; set; }
        public int? PaymentMethodId { get; set; }
    }
}
