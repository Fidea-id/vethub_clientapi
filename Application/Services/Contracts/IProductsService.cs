using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;

namespace Application.Services.Contracts
{
    public interface IProductsService : IGenericService<Products, ProductsRequest, Products, ProductsFilter>
    {
        //detail all
        Task<IEnumerable<ProductDetailsResponse>> GetProductDetailsAsync(string dbName);
        
        Task<IEnumerable<ProductCategories>> GetProductCategoriesAsync(ProductCategoriesFilter filters, string dbName);
        Task<ProductCategories> GetProductCategoryByIdAsync(int id, string dbName);
        Task<ProductCategories> AddProductCategoriesAsync(ProductsCategoriesRequest request, string dbName);
        Task<ProductCategories> UpdateProductCategoriesAsync(int id, ProductsCategoriesRequest request, string dbName);
        Task DeleteProductCategoriesAsync(int id, string dbName);
    }
}
