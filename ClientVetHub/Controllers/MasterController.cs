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
        public async Task<IActionResult> GenerateInitDB(string dbName)
        {
            try
            {
                await _masterService.GenerateTables(dbName);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("GenerateInitDBField/{dbName}")]
        public async Task<IActionResult> GenerateInitDBField(string dbName, Dictionary<string, object> dataJson)
        {
            try
            {
                await _masterService.GenerateTableField(dbName, dataJson);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
