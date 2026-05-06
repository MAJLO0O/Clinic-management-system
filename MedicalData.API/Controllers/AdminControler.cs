using Microsoft.AspNetCore.Mvc;
using MedicalData.API.DTO;
using MedicalData.Aplication.Services;
using MedicalData.API.Results;
using MedicalData.Export.Services;
using MedicalData.Import.Services;
using MedicalData.Aplication.Services.CRUD;
namespace MedicalData.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly SeedService _seedService;
        private readonly SyncService _syncService;
        private readonly ExportDataService _exportDataService;
        private readonly MedicalDataImportService _importService;
        private readonly ReadService _readService;
        private readonly BenchmarkService _benchmarkService;

        public AdminController(SeedService seedService, SyncService syncService, ExportDataService exportDataService, MedicalDataImportService importService, ReadService readService,
            BenchmarkService benchmarkService)
        {
            _seedService = seedService;
            _syncService = syncService;
            _exportDataService = exportDataService;
            _importService = importService;
            _readService = readService;
            _benchmarkService = benchmarkService;
        }

        [HttpPost("generator")]
        public async Task<IActionResult> HandleGeneratorAsync([FromBody] GenerateRequest request)
        {
            if (request.Count <= 0)
            {
                return BadRequest("count should be greater than 0");
            }
            Console.WriteLine(request.Count);
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
        [HttpGet("export")]
        public IActionResult Export()
        {
            return new FileCallbackResult("application/zip", async (outputStream, ct) => await _exportDataService.ExportZipAsync(outputStream, ct))
            {
                FileDownloadName = "export.zip"
            };
        }
        [HttpPost("import")]
        public async Task<IActionResult> ImportAsync(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            await using var stream = file.OpenReadStream();
            await _importService.ImportDataFromZipAsync(stream, ct);
            return Ok(new
            {
                message = "Import completed"
            });
        }
        [HttpPost("clean")]
        public async Task<IActionResult> CleanAsync(CancellationToken cancellationToken)
        {
            await _importService.CleanDataBaseAsync(cancellationToken);
            return Ok(new
            {
                message = "Database cleaned"
            });
        }

        [HttpGet("postgreSql/appointments")]
        public async Task<IActionResult> GetPostgreSqlAppointmentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var result = await _readService.GetAppointmentsAsync(lastId, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("postgreSql/patients")]
        public async Task<IActionResult> GetPostgreSqlPatientsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var result = await _readService.GetPatientsAsync(lastId, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("postgreSql/doctors")]
        public async Task<IActionResult> GetPostgreSqlDoctorsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var result = await _readService.GetDoctorsAsync(lastId, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("postgreSql/payments")]
        public async Task<IActionResult> GetPostgreSqlPaymentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var result = await _readService.GetPaymentsAsync(lastId, pageSize, cancellationToken);
            return Ok(result);
        }
        [HttpGet("mongoDb/appointments")]
        public async Task<IActionResult> GetMongoDbAppointmentsAsync(int lastId, int pageSize, CancellationToken cancellationToken)
        {
            var result = await _readService.GetMongoAppointmentsAsync(lastId, pageSize, cancellationToken);
            return Ok(result);
        }
        [HttpPost("benchmark")]
        public async Task<IActionResult> BenchmarkAsync([FromBody] BenchmarkRequest request, CancellationToken ct)
        {
            var result = await _benchmarkService.BenchmarkAllAsync(request.RecordCount, ct);
            return Ok(result);
        }
    }
}
