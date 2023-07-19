using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class ProductsService : GenericService<Products, ProductsRequest, Products, ProductsFilter>, IProductsService
    {
        public ProductsService(IUnitOfWork unitOfWork, IGenericRepository<Products, ProductsFilter> repository)
        : base(unitOfWork, repository)
        { }

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
                ex.Source = $"{typeof(ProductCategories).Name}Service.AddProductCategoriesAsync";
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
                ex.Source = $"{typeof(ProductCategories).Name}Service.DeleteProductCategoriesAsync";
                throw;
            }
        }

        public async Task<IEnumerable<ProductCategories>> GetProductCategoriesAsync(ProductCategoriesFilter filters, string dbName)
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
                ex.Source = $"{typeof(ProductCategories).Name}Service.GetProductCategoryByIdAsync";
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
                ex.Source = $"{typeof(ProductCategories).Name}Service.UpdateProductCategoriesAsync";
                throw;
            }
        }
    }
}
