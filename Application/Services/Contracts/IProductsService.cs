using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;

namespace Application.Services.Contracts
{
    public interface IProductsService : IGenericService<Products, ProductsRequest, Products, ProductsFilter>
    {
    }
}
