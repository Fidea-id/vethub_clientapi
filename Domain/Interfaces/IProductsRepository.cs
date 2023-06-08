using Domain.Entities.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductsRepository: IGenericRepository<Products>
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
