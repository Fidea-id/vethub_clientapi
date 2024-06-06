using Application.Services.Contracts;
using Application.Services.Implementations;
using Domain.Entities.Requests.Masters;
using Domain.Entities.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Dapper.SqlMapper;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : Controller
    {
        private readonly IMasterService _masterService;
        private readonly ILogger<MasterController> _logger;
        public MasterController(IMasterService masterService,
            ILoggerFactory loggerFactory)
        {
            _masterService = masterService;
            _logger = loggerFactory.CreateLogger<MasterController>();
        }

        [HttpGet("GenerateInitDB/{dbName}")]
        public async Task<IActionResult> GenerateInitDB(string dbName, int? version = null)
        {
            try
            {
                await _masterService.GenerateTables(dbName, version);
                return Ok(new BaseAPIResponse(200, "Success"));
            }
            catch
            {
                throw;
            }
        }
        [HttpPost("GenerateInitDBClient/{dbName}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GenerateInitDBClient(string dbName, [FromBody] RegisterClinicProfileRequest request)
        {
            try
            {
                _logger.LogInformation("Start init db");
                await _masterService.GenerateTables(dbName, null);
                _logger.LogInformation("Done init db");
                _logger.LogInformation("Start init field db");
                await _masterService.GenerateTableField(dbName);
                _logger.LogInformation("Done init field db");
                _logger.LogInformation("Start create profile and clinic data");
                var create = await _masterService.CreateClinicsProfileAsync(request, dbName);
                _logger.LogInformation("Done create profile and clinic data");
                return Ok(create);
            }
            catch(Exception ex)
            {
                _logger.LogError("Error GenerateInitDBClient-" + ex.Message);
                throw;
            }
        }
        [HttpGet("CheckSchemeDB/{version}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckSchemeDB(int version)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _masterService.UpdateTables(dbName, version);
                return Ok(new BaseAPIResponse(200, "Success"));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("GenerateInitDBField/{dbName}")]
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
