using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class MedicalRecordSnapshot
    {
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
