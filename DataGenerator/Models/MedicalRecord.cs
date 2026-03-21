using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Models
{
    public class MedicalRecord
    {
        public int AppointmentId { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
