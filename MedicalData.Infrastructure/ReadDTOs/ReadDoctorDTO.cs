using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.ReadDTOs
{
    public class ReadDoctorDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pesel { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string City { get; set; }

        public string[] Specializations { get; set; }
    }
}
