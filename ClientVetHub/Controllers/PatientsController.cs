﻿using Application.Services.Contracts;
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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientsService _patientsService;
        public PatientsController(IPatientsService patientsService)
        {
            _patientsService = patientsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PatientsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _patientsService.GetEntitiesByFilter(filters, dbName);
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
                var data = await _patientsService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Owner/{id}")]
        public async Task<IActionResult> GetByOwner(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _patientsService.ReadByOwnerIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Patients request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _patientsService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PatientsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _patientsService.UpdateAsync(id, value, dbName);
                return Ok(value);
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
                await _patientsService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] PatientsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _patientsService.ReadPatientsList(filters, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }


        [HttpPost("Statistic")]
        public async Task<IActionResult> PostStatistic([FromBody] PatientsStatisticRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _patientsService.AddPatientStatistic(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Statistic/{patientid}")]
        public async Task<IActionResult> GetPatientLatestStatistic(int patientid)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _patientsService.ReadPatientsStatisticAsync(patientid, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
