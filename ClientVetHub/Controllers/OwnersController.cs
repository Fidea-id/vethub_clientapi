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
        private readonly string _dbName;
        public OwnersController(IHttpContextAccessor httpContextAccessor, IOwnersService ownersService)
        {
            _ownersService = ownersService;
            // Retrieve the dbName from the JWT token
            _dbName = User.FindFirstValue("Entity");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var data = await _ownersService.ReadAllActiveAsync(_dbName);
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
                var data = await _ownersService.ReadByIdAsync(id, _dbName);
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
                var entities = await _ownersService.GetEntitiesByFilter(filters, _dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owners request)
        {
            try
            {
                var create = await _ownersService.CreateAsync(request, _dbName);
                return Ok(create);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Owners value)
        {
            try
            {
                await _ownersService.UpdateAsync(id, value, _dbName);
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
                await _ownersService.DeleteAsync(id, _dbName);
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
