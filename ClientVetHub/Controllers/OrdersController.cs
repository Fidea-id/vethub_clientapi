using Application.Services.Contracts;
using Application.Services.Implementations;
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

        public OrdersController(IOrdersService orderService)
        {
            _orderService = orderService;
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
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
