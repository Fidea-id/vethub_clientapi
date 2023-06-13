using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClinicsController : ControllerBase
    {
        private readonly IClinicsService _clinicsService;
        public ClinicsController(IClinicsService clinicsService)
        {
            _clinicsService = clinicsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ClinicsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _clinicsService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _clinicsService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Clinics request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _clinicsService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ClinicsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _clinicsService.UpdateAsync(id, value, dbName);
                return Ok(newData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _clinicsService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
