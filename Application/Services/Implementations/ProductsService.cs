using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Domain.Interfaces;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class ProductsService : GenericService<Products, ProductsRequest, Products, ProductsFilter>, IProductsService
    {
        public ProductsService(IUnitOfWork unitOfWork, IGenericRepository<Products, ProductsFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
