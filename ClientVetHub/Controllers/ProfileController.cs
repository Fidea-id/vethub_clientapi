using Application.Services.Contracts;
using Domain.Entities.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _authService;
        public ProfileController(IProfileService authService)
        {
            _authService = authService;
        }

        [HttpGet("User/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetUser(int id)
        {        
            // Retrieve the dbName from the JWT token
            string dbName = User.FindFirstValue("Entity");
            var user = await _authService.GetUserProfileByIdAsync(dbName, id);
            return Ok(user);
        }

        [HttpGet("testsendemail")]
        public async Task<IActionResult> Test()
        {
            await _authService.TestSendEmail();
            return Ok();
        }
    }
}