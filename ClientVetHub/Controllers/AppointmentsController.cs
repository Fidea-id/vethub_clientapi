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
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] AppointmentsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("Today")]
        public async Task<IActionResult> GetToday()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetDetailAppointmentListToday(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetStatus(dbName);
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
                var data = await _appointmentService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AppointmentsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _appointmentService.CreateRequestAsync(request, dbName);
                //map to detail
                var data = await _appointmentService.GetDetailAppointment(create.Id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("StatusChange")]
        public async Task<IActionResult> StatusChange([FromBody] AppointmentsRequestChangeStatus request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _appointmentService.ChangeAppointmentStatus(request, dbName);
                //map to detail
                var data = await _appointmentService.GetDetailAppointment(request.Id.Value, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] AppointmentsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _appointmentService.UpdateAsync(id, value, dbName);
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
                await _appointmentService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail")]
        public async Task<IActionResult> GetDetail([FromQuery] AppointmentDetailFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetDetailAppointmentList(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _appointmentService.GetDetailAppointment(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
