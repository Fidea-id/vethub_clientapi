using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class ProductsService : GenericService<Products, ProductsRequest, Products, ProductsFilter>, IProductsService
    {
        public ProductsService(IUnitOfWork unitOfWork, IGenericRepository<Products, ProductsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<IEnumerable<ProductDetailsResponse>> GetProductDetailsAsync(string dbName)
        {
            try
            {
                var data = await _unitOfWork.ProductsRepository.GetListProductDetails(dbName);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.GetProductDetailsAsync";
                throw;
            }
        }

        public async Task<ProductDetailsResponse> GetProductDetailAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.ProductsRepository.GetProductDetails(id, dbName);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.GetProductDetailAsync";
                throw;
            }
        }

        public async Task<Products> AddProductAsBundle(ProductAsBundleRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);

                var newProducts = Mapping.Mapper.Map<Products>(request);
                FormatUtil.SetIsActive<Products>(newProducts, true);
                FormatUtil.SetDateBaseEntity<Products>(newProducts);

                //add product
                var newId = await _repository.Add(dbName, newProducts);

                if (request.BundleItem != null)
                {

                    if (request.BundleItem.Count() > 0)
                    {
                        //add bundle item
                        foreach (var item in request.BundleItem)
                        {
                            var newBundle = new ProductBundles()
                            {
                                BundleId = newId,
                                ItemId = item.ItemId,
                                Quantity = item.Quantity
                            };

                            FormatUtil.SetIsActive<ProductBundles>(newBundle, true);
                            FormatUtil.SetDateBaseEntity<ProductBundles>(newBundle);

                            var newBundleId = await _unitOfWork.ProductBundlesRepository.Add(dbName, newBundle);
                        }
                    }
                }
                newProducts.Id = newId;
                return newProducts;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductService.AddProductAsBundle";
                throw;
            }
        }

        #region Product Category
        public async Task<ProductCategories> AddProductCategoriesAsync(ProductsCategoriesRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<ProductCategories>(request);
                FormatUtil.SetIsActive<ProductCategories>(entity, true);
                FormatUtil.SetDateBaseEntity<ProductCategories>(entity);

                var newId = await _unitOfWork.ProductCategoriesRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.AddProductCategoriesAsync";
                throw;
            }
        }

        public async Task DeleteProductCategoriesAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.ProductCategoriesRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _unitOfWork.ProductCategoriesRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.DeleteProductCategoriesAsync";
                throw;
            }
        }

        public async Task<DataResultDTO<ProductCategories>> GetProductCategoriesAsync(ProductCategoriesFilter filters, string dbName)
        {
            return await _unitOfWork.ProductCategoriesRepository.GetByFilter(dbName, filters);
        }

        public async Task<ProductCategories> GetProductCategoryByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.ProductCategoriesRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.GetProductCategoryByIdAsync";
                throw;
            }
        }

        public async Task<ProductCategories> UpdateProductCategoriesAsync(int id, ProductsCategoriesRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<ProductCategories>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<ProductCategories>(entity, true);

                var checkedEntity = await _unitOfWork.ProductCategoriesRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<ProductCategories, ProductCategories>(entity, checkedEntity);
                FormatUtil.SetIsActive<ProductCategories>(checkedEntity, true);
                await _unitOfWork.ProductCategoriesRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.UpdateProductCategoriesAsync";
                throw;
            }
        }
        #endregion

        #region Product Discount
        public async Task<ProductDiscounts> AddProductDiscountsAsync(ProductsDiscountsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<ProductDiscounts>(request);
                FormatUtil.SetIsActive<ProductDiscounts>(entity, true);
                FormatUtil.SetDateBaseEntity<ProductDiscounts>(entity);

                var newId = await _unitOfWork.ProductDiscountsRepository.Add(dbName, entity);
                entity.Id = newId;
                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.AddProductDiscountsAsync";
                throw;
            }
        }

        public async Task DeleteProductDiscountsAsync(int id, string dbName)
        {
            try
            {
                //get entity
                var entity = await _unitOfWork.ProductDiscountsRepository.GetById(dbName, id);
                if (entity == null) throw new Exception("Entity not found");

                await _unitOfWork.ProductDiscountsRepository.Remove(dbName, id);
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.DeleteProductDiscountsAsync";
                throw;
            }
        }

        public async Task<IEnumerable<ProductDiscountDetailResponse>> GetProductDiscountsAsync(string dbName)
        {
            return await _unitOfWork.ProductDiscountsRepository.GetProductDiscountDetail(dbName);
        }

        public async Task<ProductDiscounts> GetProductDiscountByIdAsync(int id, string dbName)
        {
            try
            {
                var data = await _unitOfWork.ProductDiscountsRepository.GetById(dbName, id);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.GetProductDiscountByIdAsync";
                throw;
            }
        }

        public async Task<ProductDiscounts> UpdateProductDiscountsAsync(int id, ProductsDiscountsRequest request, string dbName)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<ProductDiscounts>(request); // cek dulu
                FormatUtil.SetDateBaseEntity<ProductDiscounts>(entity, true);

                var checkedEntity = await _unitOfWork.ProductDiscountsRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<ProductDiscounts, ProductDiscounts>(entity, checkedEntity);
                FormatUtil.SetIsActive<ProductDiscounts>(checkedEntity, true);
                await _unitOfWork.ProductDiscountsRepository.Update(dbName, checkedEntity);
                return checkedEntity;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductsService.UpdateProductDiscountsAsync";
                throw;
            }
        }
        public async Task DeactiveDiscountAsync(int id, string dbName)
        {
            try
            {
                ProductDiscounts checkedEntity = await _unitOfWork.ProductDiscountsRepository.GetById(dbName, id);
                //convert entity
                FormatUtil.SetOppositeActive<ProductDiscounts>(checkedEntity);
                await _unitOfWork.ProductDiscountsRepository.Update(dbName, checkedEntity);
            }
            catch (Exception ex)
            {
                ex.Source = "DeactiveDiscountAsync";
                throw;
            }
        }
        #endregion
    }
}
