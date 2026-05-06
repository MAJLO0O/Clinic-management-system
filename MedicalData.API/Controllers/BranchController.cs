using MedicalData.Aplication.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/branches")]
    public class BranchController: ControllerBase
    {
        private readonly BranchService _branchService;
        public BranchController(BranchService branchService)
        {
            _branchService = branchService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken) 
        {
            var result = await _branchService.GetBranchesAsync(cancellationToken);
            return Ok(result);
        }
    }
}
