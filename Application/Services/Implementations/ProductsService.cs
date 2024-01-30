using Application.Services.Contracts;
using Application.Utils;
using AutoMapper;
using Domain.Entities.DTOs;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Models.Masters;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Services.Implementations
{
    public class ProductsService : GenericService<Products, ProductsRequest, Products, ProductsFilter>, IProductsService
    {
        private ILogger<ProductsService> _logger;
        public ProductsService(IUnitOfWork unitOfWork, IGenericRepository<Products, ProductsFilter> repository, ILoggerFactory loggerFactory)
        : base(unitOfWork, repository)
        {
            _logger = loggerFactory.CreateLogger<ProductsService>();
        }

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
        public async Task<Products> AddProducts(ProductAsBundleRequest request, string dbName, string globalId)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);

                var newProducts = Mapping.Mapper.Map<Products>(request);
                FormatUtil.SetIsActive<Products>(newProducts, true);
                FormatUtil.SetDateBaseEntity<Products>(newProducts);

                var profile = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, int.Parse(globalId));

                //add product
                var newId = await _repository.Add(dbName, newProducts);
                newProducts.Id = newId;

                //add stock
                var newStock = new ProductStocks
                {
                    ProductId = newProducts.Id,
                    Stock = request.Stock,
                    Volume = request.Volume,
                    VolumeUnit = request.VolumeUnit,
                };
                FormatUtil.SetIsActive<ProductStocks>(newStock, true);
                FormatUtil.SetDateBaseEntity<ProductStocks>(newStock);
                await _unitOfWork.ProductStockRepository.Add(dbName, newStock);

                //create new historical stock
                var newHistorical = new ProductStockHistorical()
                {
                    ProductId = newId,
                    Stock = request.Stock,
                    StockAfter = request.Stock,
                    StockBefore = 0,
                    VolumeRemaining = 0,
                    Type = "AddProduct",
                    ProfileId = profile != null ? profile.Id : 0
                };
                FormatUtil.SetIsActive<ProductStockHistorical>(newHistorical, true);
                FormatUtil.SetDateBaseEntity<ProductStockHistorical>(newHistorical);
                await _unitOfWork.ProductStockHistoricalRepository.Add(dbName, newHistorical);

                return newProducts;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductService.AddProductAsBundle";
                throw;
            }
        }

        public async Task<Products> AddProductAsBundle(ProductAsBundleRequest request, string dbName, string globalId)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);

                var newProducts = Mapping.Mapper.Map<Products>(request);
                FormatUtil.SetIsActive<Products>(newProducts, true);
                FormatUtil.SetDateBaseEntity<Products>(newProducts);
                var profile = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, int.Parse(globalId));

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

                //add stock
                var newStock = new ProductStocks
                {
                    ProductId = newProducts.Id,
                    Stock = request.Stock,
                    Volume = request.Volume,
                    VolumeUnit = request.VolumeUnit,
                };
                FormatUtil.SetIsActive<ProductStocks>(newStock, true);
                FormatUtil.SetDateBaseEntity<ProductStocks>(newStock);
                await _unitOfWork.ProductStockRepository.Add(dbName, newStock);

                //create new historical stock
                var newHistorical = new ProductStockHistorical()
                {
                    ProductId = newId,
                    Stock = request.Stock,
                    StockAfter = request.Stock,
                    StockBefore = 0,
                    VolumeRemaining = 0,
                    Type = "AddProduct",
                    ProfileId = profile != null ? profile.Id : 0
                };
                FormatUtil.SetIsActive<ProductStockHistorical>(newHistorical, true);
                FormatUtil.SetDateBaseEntity<ProductStockHistorical>(newHistorical);
                await _unitOfWork.ProductStockHistoricalRepository.Add(dbName, newHistorical);

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

        public async Task<DataResultDTO<ProductDiscountDetailResponse>> GetProductDiscountsAsync(string dbName)
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

        public async Task<Products> UpdateBundleAsync(int id, ProductAsBundleRequest request, string dbName, string globalId)
        {
            try
            {
                _logger.LogInformation("Update Product");
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Products>(request); // map dulu
                FormatUtil.SetDateBaseEntity<Products>(entity, true);

                var checkedEntity = await _unitOfWork.ProductsRepository.GetById(dbName, id);
                FormatUtil.ConvertUpdateObject<Products, Products>(entity, checkedEntity);
                FormatUtil.SetIsActive<Products>(checkedEntity, true);
                var profile = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, int.Parse(globalId));

                //update product
                await _repository.Update(dbName, checkedEntity);

                //if (request.BundleItem != null)
                //{

                //    if (request.BundleItem.Count() > 0)
                //    {
                //        //add bundle item
                //        foreach (var item in request.BundleItem)
                //        {
                //            var newBundle = new ProductBundles()
                //            {
                //                BundleId = newId,
                //                ItemId = item.ItemId,
                //                Quantity = item.Quantity
                //            };

                //            FormatUtil.SetIsActive<ProductBundles>(newBundle, true);
                //            FormatUtil.SetDateBaseEntity<ProductBundles>(newBundle);

                //            var newBundleId = await _unitOfWork.ProductBundlesRepository.Add(dbName, newBundle);
                //        }
                //    }
                //}
                //newProducts.Id = newId;

                //update stock
                _logger.LogInformation("Product Stock : " + JsonConvert.SerializeObject(request));
                if(request.Stock != null)
                {
                    _logger.LogInformation("Update Product Stock");
                    var productStock = await _unitOfWork.ProductStockRepository.WhereFirstQuery(dbName, "ProductId = " + id);
                    var stockBefore = productStock.Stock;
                    productStock.Stock = request.Stock;
                    if (!string.IsNullOrEmpty(request.VolumeUnit)){
                        productStock.VolumeUnit = request.VolumeUnit;
                    }
                    if (request.Volume != null || request.Volume != 0)
                    {
                        productStock.Volume = request.Volume;
                    }
                    FormatUtil.SetDateBaseEntity<ProductStocks>(productStock, true);
                    await _unitOfWork.ProductStockRepository.Update(dbName, productStock);

                    //create new historical stock
                    var newHistorical = new ProductStockHistorical()
                    {
                        ProductId = id,
                        Stock = productStock.Stock,
                        StockAfter = productStock.Stock,
                        StockBefore = stockBefore,
                        VolumeRemaining = productStock.VolumeRemaining,
                        Type = "UpdateProduct",
                        ProfileId = profile != null ? profile.Id : 0
                    };
                    FormatUtil.SetIsActive<ProductStockHistorical>(newHistorical, true);
                    FormatUtil.SetDateBaseEntity<ProductStockHistorical>(newHistorical);
                    await _unitOfWork.ProductStockHistoricalRepository.Add(dbName, newHistorical);
                }

                return entity;
            }
            catch (Exception ex)
            {
                ex.Source = $"ProductService.UpdateBundleAsync";
                throw;
            }
        }

        public async Task<ResponseUploadBulk> AddProductAsBulk(IEnumerable<BulkProduct> request, string dbName, string globalId)
        {
            var checkedGroups = new CheckValidDTO();
            checkedGroups.ValidationMessage = new List<string>();
            var counter = 0;
            try
            {
                _logger.LogInformation("try check product valid : " + JsonConvert.SerializeObject(request));
                checkedGroups = await _unitOfWork.ProductsRepository.CheckProductValidList(request, dbName);
                _logger.LogInformation("check product valid : " + JsonConvert.SerializeObject(checkedGroups));
                if (checkedGroups.Status == 200)
                {
                    foreach (var item in request)
                    {
                        try
                        {
                            //trim all string
                            FormatUtil.TrimObjectProperties(request);

                            var productData = new Products()
                            {
                                CategoryId = item.categoryId,
                                Description = item.description,
                                IsBundle = false,
                                Name = item.productName,
                                Price = item.price
                            };
                            FormatUtil.SetIsActive<Products>(productData, true);
                            FormatUtil.SetDateBaseEntity<Products>(productData);
                            _logger.LogInformation("new product");

                            var profile = await _unitOfWork.ProfileRepository.GetByGlobalId(dbName, int.Parse(globalId));

                            //add product
                            var newId = await _repository.Add(dbName, productData);
                            productData.Id = newId;

                            //add stock
                            var newStock = new ProductStocks
                            {
                                ProductId = productData.Id,
                                Stock = item.stock,
                                Volume = item.volume,
                                VolumeUnit = item.unit,
                            };
                            FormatUtil.SetIsActive<ProductStocks>(newStock, true);
                            FormatUtil.SetDateBaseEntity<ProductStocks>(newStock);
                            _logger.LogInformation("new product stock");
                            await _unitOfWork.ProductStockRepository.Add(dbName, newStock);

                            //create new historical stock
                            var newHistorical = new ProductStockHistorical()
                            {
                                ProductId = newId,
                                Stock = item.stock,
                                StockAfter = item.stock,
                                StockBefore = 0,
                                VolumeRemaining = 0,
                                Type = "AddProductBulk",
                                ProfileId = profile != null ? profile.Id : 0
                            };
                            FormatUtil.SetIsActive<ProductStockHistorical>(newHistorical, true);
                            FormatUtil.SetDateBaseEntity<ProductStockHistorical>(newHistorical);
                            _logger.LogInformation("new product stock historical");
                            await _unitOfWork.ProductStockHistoricalRepository.Add(dbName, newHistorical);
                            
                            counter++;
                        }
                        catch (Exception ex)
                        {
                            checkedGroups.ValidationMessage = new List<string>();
                            checkedGroups.ValidationMessage.Add($"Row {item.row}: cannot be saved. Please report to our tech for further process");
                            checkedGroups.Message = "Fail";
                        }
                    }
                    if (checkedGroups.Message == "Fail")
                    {
                        checkedGroups.Message = $"Success upload {counter} products with some data fail.";
                    }
                    else
                    {
                        checkedGroups.Message = $"Success upload {counter} products.";
                    }
                }
            }
            catch (Exception e)
            {
                checkedGroups.ValidationMessage.Add(e.Message + ". Please report to our tech for further process");
                checkedGroups.Message = "Fail";
            }
            var result = new ResponseUploadBulk()
            {
                validationMessage = checkedGroups.ValidationMessage,
                message = checkedGroups.Message,
                status = checkedGroups.Status
            };
            return result;
        }

        public async Task<IEnumerable<ProductStockHistoricalResponse>> GetProductStockHistoricalAsync(string dbName)
        {
            var getAll = await _unitOfWork.ProductStockHistoricalRepository.GetAll(dbName);
            var result = new List<ProductStockHistoricalResponse>();
            foreach(var item in getAll)
            {
                var product = await _unitOfWork.ProductsRepository.GetById(dbName, item.ProductId);
                var profile = await _unitOfWork.ProfileRepository.GetById(dbName, item.ProfileId);
                var resultItem = new ProductStockHistoricalResponse()
                {
                    Stock = item.Stock,
                    VolumeRemaining = item.VolumeRemaining,
                    Date = item.CreatedAt,
                    StockAfter = item.StockAfter, StockBefore = item.StockBefore,
                    ProductId = item.ProductId,
                    ProfileId = item.ProfileId,
                    Type = item.Type,
                    ProductName = product.Name,
                    ProfileName = profile.Name
                };
                result.Add(resultItem);
            }
            return result;
        }
        #endregion
    }
}
