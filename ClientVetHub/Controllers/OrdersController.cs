using Application.Services.Contracts;
using Application.Utils;
using DevExtreme.AspNet.Mvc;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
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
        public async Task<IActionResult> GetOrderFullAsync([FromQuery] OrderFilterRequest filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _orderService.GetOrderFullAsync(dbName, filter);
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

        [HttpGet("RevenueLogsFilter")]
        public async Task<IActionResult> GetRevenueLogAsync([FromQuery] string filterField)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var result = await _orderService.GetRevenueLogsFilterAsync(dbName, filterField);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("RevenueLogs")]
        public async Task<IActionResult> GetRevenueLogAsync([FromQuery] DataSourceLoadOptions filterParams)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var result = await _orderService.GetRevenueLogsAsync(dbName, filterParams);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("RevenueLogsPage")]
        public async Task<IActionResult> GetRevenueLogPageAsync()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var result = await _orderService.GetRevenuePagedData(dbName);
                var data = new BaseAPIResponse<int> { Message = "Success", StatusCode = 200, TotalData = result };
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
