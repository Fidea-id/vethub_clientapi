using Application.Services.Contracts;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
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

        [HttpGet("ProductStockHistorical")]
        public async Task<IActionResult> ProductStockHistorical()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _productsService.GetProductStockHistoricalAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
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
        public async Task<IActionResult> Post([FromBody] ProductAsBundleRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var idUser = User.FindFirstValue("Id");
                var create = await _productsService.AddProducts(request, dbName, idUser);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("bundle/{id}")]
        public async Task<IActionResult> PutBundle(int id, [FromBody] ProductAsBundleRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var idUser = User.FindFirstValue("Id");
                var create = await _productsService.UpdateBundleAsync(id, request, dbName, idUser);
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

        [HttpPut("Deactive/{id}")]
        public async Task<IActionResult> Deactive(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.DeactiveAsync(id, dbName);
                return Ok(new BaseAPIResponse(200, "Success"));
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


        [HttpPost("Bundle")]
        public async Task<IActionResult> AddProductAsBundleDetail(ProductAsBundleRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var idUser = User.FindFirstValue("Id");
                var entities = await _productsService.AddProductAsBundle(request, dbName, idUser);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> AddProductBulk(BulkProducts request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var idUser = User.FindFirstValue("Id");
                var entities = await _productsService.AddProductAsBulk(request.listData, dbName, idUser);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        #region ProductDetail
        [HttpGet("Detail")]
        public async Task<IActionResult> GetProductDetail()
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _productsService.GetProductDetailsAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _productsService.GetProductDetailAsync(id, dbName);
                return Ok(data);
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
                await _productsService.DeleteProductCategoriesAsync(id, dbName);
                return Ok(default(Products));
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Product Discount
        [HttpGet("Discount")]
        public async Task<IActionResult> GetDiscount()//[FromQuery] ProductDiscountsFilter filters
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var entities = await _productsService.GetProductDiscountsAsync(dbName);
                return Ok(entities);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet("Discount/{id}")]
        public async Task<IActionResult> GetDiscount(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var data = await _productsService.GetProductDiscountByIdAsync(id, dbName);
                return Ok(data);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost("Discount")]
        public async Task<IActionResult> PostDiscount([FromBody] ProductsDiscountsRequest request)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                var create = await _productsService.AddProductDiscountsAsync(request, dbName);
                return Ok(create);
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Discount/{id}")]
        public async Task<IActionResult> PutDiscount(int id, [FromBody] ProductsDiscountsRequest value)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.UpdateProductDiscountsAsync(id, value, dbName);
                return Ok(value);
            }
            catch
            {
                throw;
            }
        }

        [HttpDelete("Discount/{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.DeleteProductDiscountsAsync(id, dbName);
                return Ok(default(Products));
            }
            catch
            {
                throw;
            }
        }

        [HttpPut("Discount/Deactive/{id}")]
        public async Task<IActionResult> DeactiveDiscount(int id)
        {
            try
            {
                var dbName = User.FindFirstValue("Entity");
                await _productsService.DeactiveDiscountAsync(id, dbName);
                return Ok(new BaseAPIResponse(200, "Success"));
            }
            catch
            {
                throw;
            }
        }
        #endregion
    }
}
