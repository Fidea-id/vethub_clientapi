using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
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
            var dbName = User.FindFirstValue("Entity");
            var user = await _profileService.GetUserProfileByGlobalIdAsync(dbName, id);
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ProfileFilter filters)
        {
            var dbName = User.FindFirstValue("Entity");
            var data = await _profileService.GetEntitiesByFilter(filters, dbName);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var dbName = User.FindFirstValue("Entity");
            var data = await _profileService.ReadByIdAsync(id, dbName);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("public/{entity}")]
        public async Task<IActionResult> PublicPost(string entity, [FromBody] Profile request)
        {
            try
            {
                var dbName = entity;
                var create = await _profileService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }
        [HttpPost("{id}")]
        public async Task<IActionResult> Post(int id, [FromBody] ProfileRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var profile = Mapping.Mapper.Map<Profile>(request);
                profile.Entity = dbName;
                profile.GlobalId = id;
                var create = await _profileService.CreateAsync(profile, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProfileRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var response = await _profileService.UpdateAsync(id, value, dbName);
                return Ok(response);
            }
            catch
            {
                throw;
            }
        }


        [HttpPut("GlobalId/{id}")]
        public async Task<IActionResult> PutGlobal(int id, [FromBody] ProfileRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var response = await _profileService.UpdateUserProfileByGlobalIdAsync(dbName, value, id);
                return Ok(response);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Deactive/{id}")]
        public async Task<IActionResult> Deactive(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _profileService.DeactiveAsync(id, dbName);
                return Ok(new BaseAPIResponse(200, "Success"));
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var getPatient = await _profileService.ReadByIdAsync(id, dbName);
                await _profileService.DeleteAsync(id, dbName);
                return Ok(getPatient);
            }
            catch
            {
                throw;
            }
        }
    }
}