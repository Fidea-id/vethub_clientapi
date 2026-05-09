using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
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
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly IChartOfAccountsService _chartOfAccountsService;

        public ChartOfAccountsController(IChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ChartOfAccountsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _chartOfAccountsService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _chartOfAccountsService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChartOfAccountsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _chartOfAccountsService.CreateRequestAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ChartOfAccountsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _chartOfAccountsService.UpdateAsync(id, value, dbName);
                return Ok(newData);
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
                await _chartOfAccountsService.DeleteAsync(id, dbName);
                return Ok();
            }
            catch
            {
                throw;
            }
        }
    }
}
