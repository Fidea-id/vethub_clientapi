using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MedicalRecordsController> _logger;
        public MedicalRecordsController(IMedicalRecordService medicalRecordService, INotificationService notificationService,
            IProfileService profileService, ILogger<MedicalRecordsController> logger)
        {
            _medicalRecordService = medicalRecordService;
            _notificationService = notificationService;
            _profileService = profileService;
            _logger = logger;
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
                var url = "appointment/medicalrecord/"+create.AppointmentId;
                var notif = NotificationUtil.SetCreateNotifRequest(create.StaffId, "Create Medical Record", $"Medical Record created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                var notifOwner = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Create Medical Record", $"Medical Record created", url);
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
                _logger.LogInformation("[PAYMENT_CREATE] Stage=before_notification Tenant={Tenant} OrderId={OrderId}", dbName, request.OrderId);
                var ownerData = await _profileService.GetOwnerProfile(dbName);

                //create notif
                var url = "appointment";
                var notif = NotificationUtil.SetCreateNotifRequest(int.Parse(id), "Create Medical Payment", $"Medical payment created", url);
                var notifOwner = NotificationUtil.SetCreateNotifRequest(ownerData.Id, "Create Medical Record", $"Medical Record created", url);
                await _notificationService.CreateRequestAsync(notif, dbName);
                await _notificationService.CreateRequestAsync(notifOwner, dbName);
                _logger.LogInformation("[PAYMENT_CREATE] Stage=after_notification Tenant={Tenant} OrderId={OrderId}", dbName, request.OrderId);
                _logger.LogInformation("[PAYMENT_CREATE] Stage=success_response Tenant={Tenant} OrderId={OrderId}", dbName, request.OrderId);
                return Ok(create);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PAYMENT_CREATE] Stage=notification_or_controller Tenant={Tenant} OrderId={OrderId} ExceptionType={ExceptionType} InnerExceptionMessage={InnerExceptionMessage}",
                    User.FindFirstValue("Entity"), request?.OrderId, ex.GetType().FullName, ex.InnerException?.Message);
                throw;
            }
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetDetail(int id, [FromQuery] string? flag = null)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetDetailMedicalRecords(id, dbName, flag);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("PharmacyDetail/{id}")]
        public async Task<IActionResult> GetPharmacyDetail(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetPharmacyDetailMedicalRecords(id, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/v2/")]
        public async Task<IActionResult> GetDetailV2List([FromQuery] string? flag = null)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetDetailMedicalRecordsV2List(dbName, flag);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/v2/{id}")]
        public async Task<IActionResult> GetDetailv2(int id, [FromQuery] string? flag = null)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetDetailMedicalRecordsV2(id, dbName, flag);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("DetailMin/{id}")]
        public async Task<IActionResult> GetDetailMin(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.GetMinMedicalRecords(id, dbName);
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

        [HttpGet("Detail/History/{medId}")]
        public async Task<IActionResult> GetDetailHistory(int medId)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _medicalRecordService.GetMedicalRecordHistory(medId, dbName);
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
        [HttpPut("Prescription/{id}")]
        public async Task<IActionResult> PutPrescription(int id, [FromBody] IEnumerable<MedicalRecordsPrescriptionsRequest> request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.EditMedicalRecordPrescription(id, request, dbName);

                var userId = User.FindFirstValue("Id");

                //create notif
                var url = "";
                var notif = NotificationUtil.SetUpdateNotifRequest(int.Parse(userId), "Update Medical Record Prescription", $"Medical Record Prescription updated", url);
                _logger.LogInformation("[PHARMACY_UPDATE] Stage=before_notification Tenant={Tenant} MedicalRecordId={MedicalRecordId}", dbName, id);
                await _notificationService.CreateRequestAsync(notif, dbName);
                _logger.LogInformation("[PHARMACY_UPDATE] Stage=after_notification Tenant={Tenant} MedicalRecordId={MedicalRecordId}", dbName, id);
                _logger.LogInformation("[PHARMACY_UPDATE] Stage=success_response Tenant={Tenant} MedicalRecordId={MedicalRecordId}", dbName, id);
                return Ok(create);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PHARMACY_UPDATE] Stage=notification_or_controller Tenant={Tenant} MedicalRecordId={MedicalRecordId} ExceptionType={ExceptionType} InnerExceptionMessage={InnerExceptionMessage}",
                    User.FindFirstValue("Entity"), id, ex.GetType().FullName, ex.InnerException?.Message);
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

        [HttpGet("CloseOpname/{medId}")]
        public async Task<IActionResult> PostCloseOpname(int medId)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.PostCloseOpname(medId, dbName);
                return Ok(create);
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
        [HttpGet("ServicesReport")]
        public async Task<ActionResult<List<MedicalRecordServicesReportDto>>> GetMedicalRecordServicesReport(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var report = await _medicalRecordService.GetMedicalRecordServicesReportRawSqlAsync(dbName, startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating report: {ex.Message}");
            }
        }

        [HttpGet("DoctorPerformance")]
        public async Task<IActionResult> GetDoctorPerformance([FromQuery] int year)
        {
            if (year <= 0)
                year = DateTime.Now.Year;

            var dbName = User.FindFirstValue("Entity");
            var result = await _medicalRecordService.GetDoctorPerformance(dbName, year);

            return Ok(result);
        }

        [HttpGet("Migration/Notes")]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> GetNotesForMigration([FromQuery] int batchCount = 50)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                if (string.IsNullOrEmpty(dbName))
                {
                    return BadRequest("Database name not specified in token claims.");
                }
                var notes = await _medicalRecordService.GetNotesForMigration(dbName, batchCount);
                return Ok(notes);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Migration/Notes/{id}")]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> UpdateNoteHtml(int id, [FromBody] UpdateNoteHtmlRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                if (string.IsNullOrEmpty(dbName))
                {
                    return BadRequest("Database name not specified in token claims.");
                }
                await _medicalRecordService.UpdateNoteHtml(id, request.HtmlContent, dbName);
                return Ok();
            }
            catch
            {
                throw;
            }
        }
    }
}
