using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Infrastructure.MongoModels
{
    public class DoctorSnapshot
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Specializations { get; set; }
        public string Pesel { get; set; }
        public BranchSnapshot BranchSnapshot { get; set; }
    }
}
