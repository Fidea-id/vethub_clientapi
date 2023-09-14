using Application.Services.Contracts;
using Application.Services.Implementations;
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
    public class MedicalRecordsController : Controller
    {
        private readonly IMedicalRecordService _medicalRecordService;
        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
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
        
        [HttpPost("Detail")]
        public async Task<IActionResult> PostDetail([FromBody] MedicalRecordsDetailRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _medicalRecordService.PostAllMedicalRecords(request, dbName);
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
