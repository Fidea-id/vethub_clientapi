using Application.Services.Contracts;
using Application.Utils;
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
        private readonly INotificationService _iNotificationService;
        public AppointmentsController(IAppointmentService appointmentService, INotificationService iNotificationService)
        {
            _appointmentService = appointmentService;
            _iNotificationService = iNotificationService;
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

                //create notif
                var url = "appointment";
                var notif = NotificationUtil.SetCreateNotifRequest(data.StaffId, "New Appointment", $"Appointment from {data.OwnersName} at {data.Date.ToString("dd/MMMM/yyyy")}", url);
                await _iNotificationService.CreateRequestAsync(notif, dbName);

                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Reschedule/{id}")]
        public async Task<IActionResult> Post(int id, [FromBody] AppointmentsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");

                var newData = await _appointmentService.UpdateAsync(id, request, dbName);

                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(newData.StaffId, "Reschedule Appointment", $"Appointment reschedule", url);
                await _iNotificationService.CreateRequestAsync(notif, dbName);

                return Ok(newData);
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

                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(data.StaffId, "Update Appointment Status", $"Appointment updated to {data.StatusName}", url);
                await _iNotificationService.CreateRequestAsync(notif, dbName);

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

                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(newData.StaffId, "Update Appointment", $"Appointment updated", url);
                await _iNotificationService.CreateRequestAsync(notif, dbName);
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
                var notif = NotificationUtil.SetDeleteNotifRequest(0, "Appointment Deleted", $"Appointment deleted");
                await _iNotificationService.CreateRequestAsync(notif, dbName);
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

        [HttpGet("Invoice/{id}")]
        public async Task<IActionResult> GetDetailInvoice(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetDetailMedicalInvoice(id, dbName);
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
