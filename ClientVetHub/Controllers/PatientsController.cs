using Application.Services.Contracts;
using Domain.Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientsService _patientsService;
        private readonly string _dbName;
        public PatientsController(IHttpContextAccessor httpContextAccessor, IPatientsService patientsService)
        {
            _patientsService = patientsService;
            // Retrieve the dbName from the JWT token
            _dbName = User.FindFirstValue("Entity");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var data = await _patientsService.ReadAllActiveAsync(_dbName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var data = await _patientsService.ReadByIdAsync(id, _dbName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetEntitiesByFilter([FromQuery] Dictionary<string, object> filters)
        {
            try
            {
                var entities = await _patientsService.GetEntitiesByFilter(filters, _dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Patients request)
        {
            try
            {
                var create = await _patientsService.CreateAsync(request, _dbName);
                return Ok(create);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Patients value)
        {
            try
            {
                await _patientsService.UpdateAsync(id, value, _dbName);
                return Ok(value);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _patientsService.DeleteAsync(id, _dbName);
                return Ok(default(Patients));
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }
    }
}
