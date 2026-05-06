using MedicalData.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.ReadDTOs
{
    public class ReadAppointmentDTO
    {
        public int Id { get; set; }
        public DateTime StartingDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DoctorFullName { get; set; }
        public string PatientFullName { get; set; }
        public string AppointmentStatus { get; set; }

        public string Note { get; set; }
    }
}
