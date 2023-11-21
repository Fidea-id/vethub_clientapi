using Application.Services.Contracts;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Domain.Entities.Filters.Clients;


namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DataController : ControllerBase
    {
        private readonly IAdditionalDataService _additionalDataService;

        public DataController(IAdditionalDataService additionalDataService)
        {
            _additionalDataService = additionalDataService;
        }

        #region Dashboard
        [HttpGet("DashboardData")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadDashboardAsync(dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Clinics
        [HttpGet("ClinicsEntity/{entity}")]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> GetClinics(string entity)
        {
            try
            {
                var data = await _additionalDataService.ReadClinicsAsync(entity);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
        [HttpGet("Entity")]
        public async Task<IActionResult> GetEntity()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                return Ok(dbName);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Clinics")]
        public async Task<IActionResult> GetClinics()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadClinicsAsync(dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("ClinicsEntity/{entity}")]
        [Authorize(Roles = "Superadmin")]
        public async Task<IActionResult> PostClinics(string entity, [FromBody] ClinicsRequest request)
        {
            try
            {
                var create = await _additionalDataService.CreateClinicsAsync(request, entity);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Clinics")]
        public async Task<IActionResult> PostClinics([FromBody] ClinicsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreateClinicsAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Clinics/{id}")]
        public async Task<IActionResult> PutClinics(int id, [FromBody] ClinicsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdateClinicsAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }
        #endregion
        #region Animal
        [HttpGet("Animal")]
        public async Task<IActionResult> GetAnimal([FromQuery] NameBaseEntityFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadAnimalAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Animal/{id}")]
        public async Task<IActionResult> GetAnimal(int id)
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

        [HttpPost("Animal")]
        public async Task<IActionResult> PostAnimal([FromBody] AnimalsRequest request)
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

        [HttpPut("Animal/{id}")]
        public async Task<IActionResult> PutAnimal(int id, [FromBody] AnimalsRequest value)
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

        [HttpDelete("Animal/{id}")]
        public async Task<IActionResult> DeleteAnimal(int id)
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

        #region Breed
        [HttpGet("Breed")]
        public async Task<IActionResult> GetBreed([FromQuery] NameBaseEntityFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadBreedAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("BreedAnimal/{id}")]
        public async Task<IActionResult> GetBreedIdAnimal(int idAnimal)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadBreedByIdAnimalAsync(idAnimal, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Breed/{id}")]
        public async Task<IActionResult> GetBreed(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadBreedByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Breed")]
        public async Task<IActionResult> PostBreed([FromBody] BreedsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreateBreedAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Breed/{id}")]
        public async Task<IActionResult> PutBreed(int id, [FromBody] BreedsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdateBreedAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("Breed/{id}")]
        public async Task<IActionResult> DeleteBreed(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _additionalDataService.DeleteBreedAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Diagnose
        [HttpGet("Diagnose")]
        public async Task<IActionResult> GetDiagnose([FromQuery] NameBaseEntityFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadDiagnoseAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Diagnose/{id}")]
        public async Task<IActionResult> GetDiagnose(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadDiagnoseByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Diagnose")]
        public async Task<IActionResult> PostDiagnose([FromBody] DiagnosesRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreateDiagnoseAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Diagnose/{id}")]
        public async Task<IActionResult> PutDiagnose(int id, [FromBody] DiagnosesRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdateDiagnoseAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("Diagnose/{id}")]
        public async Task<IActionResult> DeleteDiagnose(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _additionalDataService.DeleteDiagnoseAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region PaymentMethod
        [HttpGet("PaymentMethod")]
        public async Task<IActionResult> GetPaymentMethod([FromQuery] NameBaseEntityFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadPaymentMethodAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("PaymentMethod/{id}")]
        public async Task<IActionResult> GetPaymentMethod(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _additionalDataService.ReadPaymentMethodByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("PaymentMethod")]
        public async Task<IActionResult> PostPaymentMethod([FromBody] PaymentMethodRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreatePaymentMethodAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("PaymentMethod/{id}")]
        public async Task<IActionResult> PutPaymentMethod(int id, [FromBody] PaymentMethodRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdatePaymentMethodAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("PaymentMethod/{id}")]
        public async Task<IActionResult> DeletePaymentMethod(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _additionalDataService.DeletePaymentMethodAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region PrescriptionFrequents
        [HttpGet("PrescriptionFrequents")]
        public async Task<IActionResult> GetPrescriptionFrequents([FromQuery] PrescriptionFrequentsFilter filter)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _additionalDataService.ReadPrescriptionFrequentsAllAsync(filter, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("PrescriptionFrequents")]
        public async Task<IActionResult> PostPrescriptionFrequents([FromBody] PrescriptionFrequentsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _additionalDataService.CreatePrescriptionFrequentsAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("PrescriptionFrequents/{id}")]
        public async Task<IActionResult> PutPrescriptionFrequents(int id, [FromBody] PrescriptionFrequentsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var newData = await _additionalDataService.UpdatePrescriptionFrequentsAsync(id, value, dbName);
                return Ok(newData);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("PrescriptionFrequents/{id}")]
        public async Task<IActionResult> DeletePrescriptionFrequents(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _additionalDataService.DeletePrescriptionFrequentsAsync(id, dbName);
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
