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
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }
        #region Product
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ProductsFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _productsService.GetEntitiesByFilter(filters, dbName);
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
                var data = await _productsService.ReadByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Products request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _productsService.CreateAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.UpdateAsync(id, value, dbName);
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
                await _productsService.DeleteAsync(id, dbName);
                return Ok(default(Products));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Product Category
        [HttpGet("Category")]
        public async Task<IActionResult> GetCategory([FromQuery] ProductCategoriesFilter filters)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _productsService.GetProductCategoriesAsync(filters, dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Category/{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _productsService.GetProductCategoryByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Category")]
        public async Task<IActionResult> PostCategory([FromBody] ProductsCategoriesRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _productsService.AddProductCategoriesAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Category/{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] ProductsCategoriesRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.UpdateProductCategoriesAsync(id, value, dbName);
                return Ok(value);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("Category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.DeleteAsync(id, dbName);
                return Ok(default(Products));
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
