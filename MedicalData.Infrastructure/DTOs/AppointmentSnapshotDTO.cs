using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.DTOs
{
    public class AppointmentSnapshotDTO
    {
        public int Id { get; set; }
        public DateTime StartingDateTime { get; set; }

        public DateTime CreatedAt { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public int AppointmentStatusId { get; set; }

    }
}
