using System.ComponentModel.DataAnnotations;

namespace MedicalData.API.CrudDTOs
{
    public class UpdateAppointmentRequest
    {
        [Required]
        public DateTime StartingDateTime { get; set; }
    }
}
