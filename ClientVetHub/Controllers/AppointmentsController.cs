using Application.Services.Contracts;
using Application.Utils;
using Domain.Constants;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Utils;
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
        private readonly IProfileService _profileService;
        private readonly INotificationService _iNotificationService;
        public AppointmentsController(IAppointmentService appointmentService, INotificationService iNotificationService, IProfileService profileService)
        {
            _appointmentService = appointmentService;
            _iNotificationService = iNotificationService;
            _profileService = profileService;
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

        [HttpGet("InvoiceEmail/{id}")]
        public async Task<IActionResult> PostInvoiceEmail(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _appointmentService.SendInvoiceEmail(dbName, id);

                return Ok();
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
                return Ok(default(Appointments));
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
                if (User.IsInRole(RoleUserConstant.DOCTOR))
                {
                    var id = User.FindFirstValue("Id");
                    int idglobal = 0;
                    var tryParseId = int.TryParse(id, out idglobal);
                    if (tryParseId)
                    {
                        var userClient = await _profileService.GetUserProfileByGlobalIdAsync(dbName, idglobal);
                        if (userClient != null)
                        {
                            filter.StaffId = userClient.Id;
                        }
                    }
                }
                var entities = await _appointmentService.GetDetailAppointmentList(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
        
        [HttpGet("Detail/Medical")]
        public async Task<IActionResult> GetDetailMedical([FromQuery] AppointmentDetailFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _appointmentService.GetDetailAppointmentMedicalList(filter, dbName);
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


        [HttpGet("Type")]
        public async Task<IActionResult> GetTypes()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _appointmentService.ReadAllAppointmentsTypeAsync(dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Type")]
        public async Task<IActionResult> PostType([FromBody] AppointmentsType request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _appointmentService.AddAppointmentsTypeAsync(dbName, request);

                return Ok(create);
            }
            catch
            {
                throw;
            }
        }
        [HttpPut("Type/{id}")]
        public async Task<IActionResult> PutType(int id, [FromBody] AppointmentsType value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _appointmentService.UpdateAppointmentsTypeAsync(dbName, id, value);

                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("Type/{id}")]
        public async Task<IActionResult> DeleteType(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _appointmentService.DeleteAppointmentsTypeAsync(dbName, id);
                return Ok(default(AppointmentsType));
            }
            catch
            {
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("PublicInvoice")]
        public async Task<IActionResult> GetPublicInvoice(string e, int d)
        {
            try
            {
                var entities = await _appointmentService.GetDetailMedicalInvoice(d, e);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
    }
}
