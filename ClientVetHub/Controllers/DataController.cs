using Application.Services.Contracts;
using Domain.Entities.Filters;
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
    public class DataController : ControllerBase
    {
        private readonly IAdditionalDataService _additionalDataService;

        public DataController(IAdditionalDataService additionalDataService)
        {
            _additionalDataService = additionalDataService;
        }

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
    }
}
