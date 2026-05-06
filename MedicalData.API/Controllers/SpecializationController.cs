using MedicalData.Aplication.Services.CRUD;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/specializations")]
    public class SpecializationController : ControllerBase
    {
        private readonly SpecializationService _specializationService;
        public SpecializationController(SpecializationService specializationService)
        {
            _specializationService = specializationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _specializationService.GetSpecializationsAsync(ct);
            return Ok(result);
        }
        
       
    }
}
