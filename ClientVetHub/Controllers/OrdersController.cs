using Application.Services.Contracts;
using Application.Services.Implementations;
using Application.Utils;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : Controller
    {
        private readonly IOrdersService _orderService;
        private readonly INotificationService _notificationService;
        private readonly IProfileService _profileService;

        public OrdersController(IOrdersService orderService, INotificationService notificationService, IProfileService profileService)
        {
            _orderService = orderService;
            _notificationService = notificationService;
            _profileService = profileService;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetOrderDashboardAsync()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetOrderDashboardAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("Full")]
        public async Task<IActionResult> GetOrderFullAsync()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetOrderFullAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("FullMonth")]
        public async Task<IActionResult> GetOrderFullMonthAsync()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetOrderFullAsync(dbName, true);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Full/{id}")]
        public async Task<IActionResult> GetOrderFullAsync(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetOrderFullByIdAsync(id, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Full")]
        public async Task<IActionResult> Post([FromBody] OrderFullRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _orderService.CreateOrderFullAsync(request, dbName);
                var ownerData = await _profileService.GetOwnerProfile(dbName);

                //create notif
                var url = "";
                var notif = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Create Order", $"Order type {data.Type} created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
        [HttpPost("Payment")]
        public async Task<IActionResult> PostPayment([FromBody] OrdersPaymentRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _orderService.AddOrdersPaymentAsync(request, dbName);
                var ownerData = await _profileService.GetOwnerProfile(dbName);

                //create notif
                var url = "";
                var notif = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Order Payment Created", $"Order type {data.Type} payment created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {

            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _orderService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("RevenueLogs")]
        public async Task<IActionResult> GetRevenueLogAsync()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetRevenueLogAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
    }
}
