﻿using Domain.Entities.Filters.Clients;
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

        Task<Products> AddProductAsBundle(ProductAsBundleRequest request, string dbName);

        Task<IEnumerable<ProductCategories>> GetProductCategoriesAsync(ProductCategoriesFilter filters, string dbName);
        Task<ProductCategories> GetProductCategoryByIdAsync(int id, string dbName);
        Task<ProductCategories> AddProductCategoriesAsync(ProductsCategoriesRequest request, string dbName);
        Task<ProductCategories> UpdateProductCategoriesAsync(int id, ProductsCategoriesRequest request, string dbName);
        Task DeleteProductCategoriesAsync(int id, string dbName);

        Task<IEnumerable<ProductDiscountDetailResponse>> GetProductDiscountsAsync(ProductDiscountsFilter filters, string dbName);
        Task<ProductDiscounts> GetProductDiscountByIdAsync(int id, string dbName);
        Task<ProductDiscounts> AddProductDiscountsAsync(ProductsDiscountsRequest request, string dbName);
        Task<ProductDiscounts> UpdateProductDiscountsAsync(int id, ProductsDiscountsRequest request, string dbName);
        Task DeleteProductDiscountsAsync(int id, string dbName);
        Task DeactiveDiscountAsync(int id, string dbName);
    }
}
