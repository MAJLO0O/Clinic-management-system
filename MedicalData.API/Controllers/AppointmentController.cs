using MedicalData.API.CrudDTOs;
using MedicalData.Aplication.Services.CRUD;
using Microsoft.AspNetCore.Mvc;
using MedicalData.Domain.Models;

namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController
    {
        private readonly AppointmentService _appointmentService;
        public AppointmentController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        [HttpPut("{id}")]
        public async Task<bool> Update(int id,[FromBody] UpdateAppointmentRequest updateAppointment,CancellationToken ct)
        {
            Appointment appointment = new()
            {
                StartingDateTime = updateAppointment.StartingDateTime
            };
            return await _appointmentService.UpdateAppointmentAsync(id, appointment, ct);
        }

    }
}
