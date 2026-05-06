using System.ComponentModel.DataAnnotations;

namespace MedicalData.API.CrudDTOs
{
    public class CreateDoctorRequest
    {
        [Required]
        [StringLength(50, MinimumLength =3)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be 11 digits")]
        public string Pesel { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public int BranchId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one specialization required")]
        [MaxLength(3, ErrorMessage = "Maximum 3 specializations")]
        public List<int> SpecializationIds { get; set; } = new();
    }
    public class UpdateDoctorRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; }
        [Required]
        [Phone]
        public string Phone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public int BranchId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "At least one specialization required")]
        [MaxLength(3, ErrorMessage = "Maximum 3 specializations")]
        public List<int> SpecializationIds { get; set; } = new();
    }
}
