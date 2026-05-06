using MedicalData.Aplication.CrudDTOs;
using MedicalData.Aplication.Services.CRUD;
using MedicalData.Domain.Models;
using Microsoft.AspNetCore.Mvc;
namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientController(PatientService patientService)
        {
            _patientService = patientService;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request, CancellationToken ct)
        {
            var patient = new Patient
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Pesel = request.Pesel,
                DateOfBirth = request.DateOfBirth.ToDateTime(new TimeOnly(0, 0)),
                Phone = request.Phone,
                Email = request.Email
            };
            await _patientService.CreatePatientAsync(patient, ct);
            return Created();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
        {
            var patient = new Patient
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Email = request.Email
            };
            var success = await _patientService.UpdatePatientAsync(id, patient, ct);
            return success ? NoContent() : NotFound();
        }
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeletePatient(int id, CancellationToken ct)
        {
            var success  = await _patientService.DeletePatientAsync(id, ct);
            return success ? NoContent() : NotFound();
        }
    }
}
