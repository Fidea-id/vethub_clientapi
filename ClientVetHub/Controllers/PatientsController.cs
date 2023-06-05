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
        public PatientsController(IHttpContextAccessor httpContextAccessor, IPatientsService patientsService)
        {
            _patientsService = patientsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _patientsService.ReadAllActiveAsync(dbName);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _patientsService.ReadByIdAsync(id, dbName);
            return Ok(data);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetEntitiesByFilter([FromQuery] Dictionary<string, object> filters)
        {
            try
            {
                // Retrieve the dbName from the JWT token
                string dbName = User.FindFirstValue("Entity");
                var entities = await _patientsService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Patients request)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var create = await _patientsService.CreateAsync(request, dbName);
            return Ok(create);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Patients value)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _patientsService.UpdateAsync(id, value, dbName);
            return Ok(value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _patientsService.DeleteAsync(id, dbName);
            return Ok(default(Patients));
        }
    }
}
