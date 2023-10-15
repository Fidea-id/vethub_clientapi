using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;

namespace Application.Services.Contracts
{
    public interface IOwnersService : IGenericService<Owners, OwnersRequest, Owners, OwnersFilter>
    {
        Task<Owners> CreateOwnersPetsAsync(OwnersPetsRequest request, string dbName);
        Task<OwnerStatistic> GetOwnerStatisticAsync(int ownerId, string dbName);
    }
}
