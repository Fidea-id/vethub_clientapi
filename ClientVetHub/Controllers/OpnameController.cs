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
    public class OpnameController : Controller
    {
        private readonly IOpnameService _opnameService;

        public OpnameController(IOpnameService opnameService)
        {
            _opnameService = opnameService;
        }

        #region Opnames
        [HttpGet]
        public async Task<IActionResult> GetOpnames([FromQuery] OpnamesFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _opnameService.GetEntitiesByFilter(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOpnames(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _opnameService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostOpnames([FromBody] OpnamesRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _opnameService.CreateRequestAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOpnames(int id, [FromBody] OpnamesRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _opnameService.UpdateAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOpnames(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _opnameService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region OpnamePatients
        [HttpGet("OpnamePatients")]
        public async Task<IActionResult> GetOpnamePatients([FromQuery] OpnamePatientsFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _opnameService.ReadOpnamePatientsAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("OpnamePatients/{id}")]
        public async Task<IActionResult> GetOpnamePatients(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _opnameService.ReadOpnamePatientsByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }


        [HttpGet("OpnamePatients/Medical/{id}")]
        public async Task<IActionResult> GetOpnamePatientsByMedId(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _opnameService.ReadOpnamePatientsByMedIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("OpnamePatients/Opname/{id}")]
        public async Task<IActionResult> GetOpnamePatientsByOpnameId(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _opnameService.ReadOpnamePatientsByOpnameIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }


        [HttpPost("OpnamePatients")]
        public async Task<IActionResult> PostOpnamePatients([FromBody] OpnamePatientsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _opnameService.CreateOpnamePatientsAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("OpnamePatients/{id}")]
        public async Task<IActionResult> PutOpnamePatients(int id, [FromBody] OpnamePatientsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _opnameService.UpdateOpnamePatientsAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("OpnamePatients/{id}")]
        public async Task<IActionResult> DeleteOpnamePatients(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _opnameService.DeleteOpnamePatientsAsync(id, dbName);
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
