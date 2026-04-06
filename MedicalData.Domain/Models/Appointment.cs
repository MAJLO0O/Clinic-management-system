using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Domain.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [Required]
        public DateTime StartingDateTime { get; set; }
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public int PatientId { get; set; }
        [Required]
        public int AppointmentStatusId { get; set; }
    }
}
