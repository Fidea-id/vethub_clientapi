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
    public class OwnersController : ControllerBase
    {
        private readonly IOwnersService _ownersService;
        public OwnersController(IHttpContextAccessor httpContextAccessor, IOwnersService ownersService)
        {
            _ownersService = ownersService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _ownersService.ReadAllActiveAsync(dbName);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _ownersService.ReadByIdAsync(id, dbName);
            return Ok(data);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetEntitiesByFilter([FromQuery] Dictionary<string, object> filters)
        {
            try
            {
                // Retrieve the dbName from the JWT token
                string dbName = User.FindFirstValue("Entity");
                var entities = await _ownersService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owners request)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var create = await _ownersService.CreateAsync(request, dbName);
            return Ok(create);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Owners value)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _ownersService.UpdateAsync(id, value, dbName);
            return Ok(value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _ownersService.DeleteAsync(id, dbName);
            return Ok(default(Patients));
        }
    }
}
