using Application.Services.Contracts;
using Domain.Entities.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [HttpGet("GenerateInitDB/{dbName}")]
        public async Task<IActionResult> GenerateInitDB(string dbName, int? version = null)
        {
            try
            {
                await _masterService.GenerateTables(dbName, version);
                return Ok("Success");
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("CheckSchemeDB")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckSchemeDB()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _masterService.GenerateTables(dbName);
                return Ok(new BaseAPIResponse(200, "Success"));
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
                return Ok(new BaseAPIResponse(200, "Success"));
            }
            catch
            {
                throw;
            }
        }
    }
}
