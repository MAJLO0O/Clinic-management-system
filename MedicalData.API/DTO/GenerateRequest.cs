using System.ComponentModel.DataAnnotations;

namespace MedicalData.API.DTO
{
    public class GenerateRequest
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int Count { get; set; }
    }
}
