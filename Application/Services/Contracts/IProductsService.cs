using Domain.Entities.DTOs;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IProductsService : IGenericService<Products, ProductsRequest, Products, ProductsFilter>
    {
        //detail all
        Task<IEnumerable<ProductDetailsResponse>> GetProductDetailsAsync(string dbName);
        Task<ProductDetailsResponse> GetProductDetailAsync(int id, string dbName);
        Task<IEnumerable<ProductStockHistoricalResponse>> GetProductStockHistoricalAsync(string dbName);

        Task<Products> AddProducts(ProductAsBundleRequest request, string dbName, string globalId);
        Task<Products> AddProductAsBundle(ProductAsBundleRequest request, string dbName, string globalId);
        Task<ResponseUploadBulk> AddProductAsBulk(IEnumerable<BulkProduct> request, string dbName, string globalId);
        Task<Products> UpdateBundleAsync(int id, ProductAsBundleRequest request, string dbName, string globalId);

        Task<DataResultDTO<ProductCategories>> GetProductCategoriesAsync(ProductCategoriesFilter filters, string dbName);
        Task<ProductCategories> GetProductCategoryByIdAsync(int id, string dbName);
        Task<ProductCategories> AddProductCategoriesAsync(ProductsCategoriesRequest request, string dbName);
        Task<ProductCategories> UpdateProductCategoriesAsync(int id, ProductsCategoriesRequest request, string dbName);
        Task DeleteProductCategoriesAsync(int id, string dbName);

        Task<DataResultDTO<ProductDiscountDetailResponse>> GetProductDiscountsAsync(string dbName);
        Task<ProductDiscounts> GetProductDiscountByIdAsync(int id, string dbName);
        Task<ProductDiscounts> AddProductDiscountsAsync(ProductsDiscountsRequest request, string dbName);
        Task<ProductDiscounts> UpdateProductDiscountsAsync(int id, ProductsDiscountsRequest request, string dbName);
        Task DeleteProductDiscountsAsync(int id, string dbName);
        Task DeactiveDiscountAsync(int id, string dbName);
    }
}
