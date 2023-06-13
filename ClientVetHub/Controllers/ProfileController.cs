﻿using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientVetHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var dbName = User.FindFirstValue("Entity");
            var user = await _profileService.GetUserProfileByIdAsync(dbName, id);
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ProfileFilter filters)
        {
            var dbName = User.FindFirstValue("Entity");
            var data = await _profileService.ReadAllActiveAsync(dbName);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var dbName = User.FindFirstValue("Entity");
            var data = await _profileService.ReadByIdAsync(id, dbName);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("public/{entity}")]
        public async Task<IActionResult> PublicPost(string entity, [FromBody] Profile request)
        {
            try
            {
                var dbName = entity;
                var create = await _profileService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Profile request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _profileService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProfileRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _profileService.UpdateAsync(id, value, dbName);
                return Ok(value);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _profileService.DeleteAsync(id, dbName);
                return Ok(default(Patients));
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred.");
            }
        }
    }
}