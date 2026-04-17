using Microsoft.AspNetCore.Mvc;
using MedicalData.API.DTO;
using MedicalData.Aplication.Services;
namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController: ControllerBase
    {
        private readonly SeedService _seedService;
        private readonly SyncService _syncService;

        public AdminController(SeedService seedService, SyncService syncService)
        {
            _seedService = seedService;
            _syncService = syncService;
        }

        [HttpPost("generator")]
        public async Task<IActionResult> HandleGeneratorAsync([FromBody] GenerateRequest request)
        {
            if (request.Count <= 0)
            {
                return BadRequest("count should be greater than 0");
            }
            await _seedService.SeedAllAsync(request.Count);
            return Ok(new { processedCount = request.Count });
        }
        [HttpPost("sync")]
        public async Task<IActionResult> HandleSyncAsync()
        {
            await _syncService.SyncAsync();
            return Ok(new
            {
                message = "Sync completed"
            });
        }
    }
}
