using System.ComponentModel.DataAnnotations;

namespace MedicalData.API.CrudDTOs
{
    public class UpdatePaymentRequest
    {
        [Required]
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public int StatusId { get; set; }
    }
}
