using Domain.Entities.Responses;

namespace Domain.Interfaces
{
    public interface IProductsRepository : IGenericRepository<Products, ProductsFilter>
    {
        Task AddProductDiscounts(ProductDiscounts discount, string dbName);
        Task AddProductCategories(ProductCategories categories, string dbName);
        Task AddProductBundles(ProductBundles bundles, string dbName);
        Task UpdateProductDiscounts(int id, ProductDiscounts discount, string dbName);
        Task UpdateProductBundles(int id, ProductBundles bundles, string dbName);
        Task UpdateProductCategories(int id, ProductCategories categories, string dbName);
        Task DeleteProductDiscounts(int id, string dbName);
        Task DeleteProductBundles(int id, string dbName);

        Task<ProductDetailsResponse> GetProductDetails(int id, string dbName);
        Task<IEnumerable<ProductDetailsResponse>> GetListProductDetails(string dbName);
    }
}
