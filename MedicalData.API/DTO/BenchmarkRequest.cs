using System.ComponentModel.DataAnnotations;

namespace MedicalData.API.DTO
{
    public class BenchmarkRequest
    {
        [Required]
        [AllowedValues(100, 1000, 5000, 50000)]
        public int RecordCount { get; set; }
    }
}
