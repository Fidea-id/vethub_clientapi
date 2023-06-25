using Domain.Entities.Filters.Clients;
using Domain.Entities.Requests.Clients;

namespace Application.Services.Contracts
{
    public interface IServicesService : IGenericService<Domain.Entities.Models.Clients.Services, ServicesRequest, Domain.Entities.Models.Clients.Services, ServicesFilter>
    {
    }
}
