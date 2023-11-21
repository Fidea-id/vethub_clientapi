using Application.Services.Contracts;
using Domain.Interfaces.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly INotificationService _notificationService;

        public NotificationController(IProfileService profileService, INotificationService notificationService)
        {
            _profileService = profileService;
            _notificationService = notificationService;
        }

        [HttpGet("GetAllNotif")]
        public async Task<IActionResult> GetAllListNotif()
        {
            var dbName = User.FindFirstValue("Entity");
            var id = User.FindFirstValue("Id");
            var profile = await _profileService.GetUserProfileByGlobalIdAsync(dbName, int.Parse(id));
            var notif = await _notificationService.GetAll(dbName, profile.Id);
            return Ok(notif);
        }
        [HttpGet("GetRecentNotif")]
        public async Task<IActionResult> GetRecentNotif()
        {
            var dbName = User.FindFirstValue("Entity");
            var id = User.FindFirstValue("Id");
            var profile = await _profileService.GetUserProfileByGlobalIdAsync(dbName, int.Parse(id));
            var notif = await _notificationService.GetRecent(dbName, profile.Id);
            return Ok(notif);
        }
    }
}
