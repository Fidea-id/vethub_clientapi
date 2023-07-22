using Application.Services.Contracts;
using Application.Services.Implementations;
using Domain.Entities;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Models.Masters;
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
    public class AdditionalDataController : ControllerBase
    {
        private readonly IAdditionalDataService _additionalDataService;

        public AdditionalDataController(IAdditionalDataService additionalDataService)
        {
            _additionalDataService = additionalDataService;
        }

        #region Animal
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadAnimalAllAsync(dbName);
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
                var data = await _additionalDataService.ReadAnimalByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Animals request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreateAnimalAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Animals value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdateAnimalAsync(id, value, dbName);
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
                await _additionalDataService.DeleteAnimalAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
