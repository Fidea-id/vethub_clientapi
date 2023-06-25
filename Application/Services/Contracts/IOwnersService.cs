using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;

namespace Application.Services.Contracts
{
    public interface IOwnersService : IGenericService<Owners, OwnersRequest, Owners, OwnersFilter>
    {
        Task<Owners> CreateOwnersPetsAsync(OwnersPetsRequest request, string dbName);
    }
}
