using Application.Services.Contracts;
using Domain.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {        
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var user = await _profileService.GetUserProfileByIdAsync(dbName, id);
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _profileService.ReadAllActiveAsync(dbName);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var data = await _profileService.ReadByIdAsync(id, dbName);
            return Ok(data);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetEntitiesByFilter([FromQuery] Dictionary<string, object> filters)
        {
            try
            {
                // Retrieve the dbName from the JWT token
                string dbName = User.FindFirstValue("Entity");
                var entities = await _profileService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                // Handle any errors and return an appropriate response
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile request)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var create = await _profileService.CreateAsync(request, dbName);
            return Ok(create);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Profile value)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _profileService.UpdateAsync(id, value, dbName);
            return Ok(value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            await _profileService.DeleteAsync(id, dbName);
            return Ok(default(Patients));
        }
    }
}