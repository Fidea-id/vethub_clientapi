using Domain.Entities.Filters;
using Domain.Entities.Requests;

namespace Application.Services.Contracts
{
    public interface IServicesService : IGenericService<Domain.Entities.Models.Services, ServicesRequest, Domain.Entities.Models.Services, ServicesFilter>
    {
    }
}
