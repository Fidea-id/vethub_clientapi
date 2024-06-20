using Application.Services.Contracts;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(IProfileService profileService, INotificationService notificationService, ICurrentUserService currentUserService, ILogger<NotificationController> logger)
        {
            _currentUserService = currentUserService;
            _logger = logger;
            _profileService = profileService;
            _notificationService = notificationService;
        }

        [HttpGet("GetAllNotif")]
        public async Task<IActionResult> GetAllListNotif()
        {
            var dbName = User.FindFirstValue("Entity");

            var profileId = await _currentUserService.UserId;
            var notif = await _notificationService.GetAll(dbName, profileId);
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
