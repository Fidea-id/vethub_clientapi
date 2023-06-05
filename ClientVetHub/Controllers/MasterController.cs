using Application.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet("GenerateInitDB")]
        public async Task<IActionResult> GenerateInitDB(string dbName)
        {
            await _masterService.GenerateTables(dbName);
            return Ok();
        }
    }
}
