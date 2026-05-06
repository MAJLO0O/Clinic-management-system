using MedicalData.Aplication.Services.CRUD;
using Microsoft.AspNetCore.Mvc;
using MedicalData.Aplication.CrudDTOs;
using MedicalData.Domain.Models;
using MedicalData.API.CrudDTOs;
using MedicalData.Infrastructure.DTOs;
namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorService _doctorService;
        public DoctorController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest dto, CancellationToken ct)
        {
            var doctor = new Doctor()
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Pesel = dto.Pesel,
                Phone = dto.Phone,
                Email = dto.Email,
                BranchId = dto.BranchId,
            };
            var specializationIds = dto.SpecializationIds;
            await _doctorService.CreateDoctorAsync(doctor,specializationIds.Distinct().ToList(), ct);
            return Created();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorRequest updateDoctor,CancellationToken ct)
        {
            var doctor = new Doctor
            {
                Id = id,
                FirstName = updateDoctor.FirstName,
                LastName = updateDoctor.LastName,
                Phone = updateDoctor.Phone,
                Email = updateDoctor.Email,
                BranchId = updateDoctor.BranchId,
            };
            var specializationsIds = updateDoctor.SpecializationIds;
            var result = await _doctorService.UpdateDoctorAsync(id, doctor,specializationsIds.Distinct().ToList(), ct);
            return result ? NoContent() : NotFound();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id, CancellationToken ct)
        {
            var result = await _doctorService.DeleteDoctorAsync(id, ct);
            return result ? NoContent() : NotFound();

        }
    }
}
