using Application.Services.Contracts;
using Application.Services.Implementations;
using Application.Utils;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MedicalRecordsController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        private readonly INotificationService _notificationService;
        private readonly IProfileService _profileService;
        public MedicalRecordsController(IMedicalRecordService medicalRecordService, INotificationService notificationService, IProfileService profileService)
        {
            _medicalRecordService = medicalRecordService;
            _notificationService = notificationService;
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] MedicalRecordsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetEntitiesByFilter(filters, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("PatientDiagnose/{patientId}")]
        public async Task<IActionResult> GetPatientDiagnose(int patientId)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetPatientDiagnose(patientId, dbName);
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
                var data = await _medicalRecordService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("RequirementData/{id}")]
        public async Task<IActionResult> GetRequirementData(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _medicalRecordService.GetMedicalRecordRequirement(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MedicalRecordsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.CreateRequestAsync(request, dbName);
                var ownerData = await _profileService.GetOwnerProfile(dbName);

                //create notif
                var url = "appointment";
                var notif = NotificationUtil.SetCreateNotifRequest(create.StaffId, "Create Medical Record", $"Medical Record created", url);
                var notifOwner = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Create Medical Record", $"Medical Record created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                await _notificationService.CreateRequestAsync(notifOwner, dbName);
                return Ok(create);
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
                var id = User.FindFirstValue("Id");
                var create = await _medicalRecordService.AddOrdersPaymentAsync(request, dbName);
                var ownerData = await _profileService.GetOwnerProfile(dbName);

                //create notif
                var url = "appointment";
                var notif = NotificationUtil.SetCreateNotifRequest(int.Parse(id), "Create Medical Payment", $"Medical payment created", url);
                var notifOwner = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Create Medical Record", $"Medical Record created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                await _notificationService.CreateRequestAsync(notifOwner, dbName);
                return Ok(create);
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
                var create = await _medicalRecordService.GetDetailMedicalRecords(id, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Payment/{id}")]
        public async Task<IActionResult> GetMedicalPayment(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetOrdersPaymentAsync(id, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/Owner/{ownerId}")]
        public async Task<IActionResult> GetBookingHistoryByOwner(int ownerId)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetBookingHistoryByOwner(ownerId, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/Patient/{patientId}")]
        public async Task<IActionResult> GetBookingHistoryByPatient(int patientId)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetBookingHistoryByPatient(patientId, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Detail")]
        public async Task<IActionResult> PostDetail([FromBody] MedicalRecordsDetailRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.PostAllMedicalRecords(request, dbName);


                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(create.Staff.Id, "Update Medical Record", $"Medical Record updated", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] MedicalRecordsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _medicalRecordService.UpdateAsync(id, value, dbName);

                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(newData.StaffId, "Update Medical Record", $"Medical Record updated", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
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
                await _medicalRecordService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Notes")]
        public async Task<IActionResult> PostNotes([FromBody] MedicalRecordsNotesRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var email = User.FindFirstValue(ClaimTypes.Email);
                var create = await _medicalRecordService.PostMedicalRecordsNotes(request, email, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Notes/{id}")]
        public async Task<IActionResult> PutNotes(int id, [FromBody] MedicalRecordsNotesRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var email = User.FindFirstValue(ClaimTypes.Email);
                var create = await _medicalRecordService.PutMedicalRecordsNotes(id, request, email, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Notes/{id}")]
        public async Task<IActionResult> GetNotesByMedicalRecordId(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetMedicalRecordsNotes(id, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }
        [HttpDelete("Notes/{id}")]
        public async Task<IActionResult> DeleteNotes(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _medicalRecordService.DeleteMedicalRecordsNotes(id, dbName);
                return Ok(default(MedicalRecordsNotes));
            }
            catch
            {
                throw;
            }
        }
    }
}
