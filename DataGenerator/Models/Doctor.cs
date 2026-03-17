using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenerator.Models
{
    public class Doctor
    {
        [Required]
        [MinLength(2),MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(2),MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        [StringLength(11)]
        public string Pesel { get; set; }
        [Required]
        [MinLength(9), MaxLength(15)]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(5),MaxLength(100)]
        public string Email { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        [Required]
        public int BranchId { get; set; }
    }
}
