using Application.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IMasterService _masterService;
        public MasterController(IMasterService masterService)
        {
            _masterService = masterService;
        }

        [HttpGet("GenerateInitDB/{dbName}")]
        public async Task<IActionResult> GenerateInitDB(string dbName, int? version = null)
        {
            try
            {
                await _masterService.GenerateTables(dbName, version);
                return Ok();
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("GenerateInitDBField/{dbName}")]
        public async Task<IActionResult> GenerateInitDBField(string dbName)
        {
            try
            {
                await _masterService.GenerateTableField(dbName);
                return Ok();
            }
            catch
            {
                throw;
            }
        }
    }
}
