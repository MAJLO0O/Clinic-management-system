using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.DTOs
{
    public  class ExportDoctorSpecializationDTO
    {
        public int DoctorId { get; set; }
        public int SpecializationId { get; set; }
    }
}
