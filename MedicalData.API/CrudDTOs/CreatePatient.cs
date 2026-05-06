using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalData.Aplication.CrudDTOs
{
    public class CreatePatientRequest
    {
        [Required]
        [Length(3, 50)]
        public string FirstName { get; set; }
        [Required]
        [Length(3, 50)]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be 11 digits")]
        public string Pesel { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [EmailAddress]
        [Length(5, 100)]
        public string Email { get; set; }

    }
    public class UpdatePatientRequest
    {
        [Required]
        [Length(3, 50)]
        public string FirstName { get; set; }
        [Required]
        [Length(3, 50)]
        public string LastName { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [EmailAddress]
        [Length(5, 100)]
        public string Email { get; set; }

    }
}

