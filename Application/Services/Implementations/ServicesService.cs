using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Requests;
using Domain.Interfaces;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class ServicesService : GenericService<Domain.Entities.Models.Clients.Services, ServicesRequest, Domain.Entities.Models.Clients.Services, ServicesFilter>, IServicesService
    {
        public ServicesService(IUnitOfWork unitOfWork, IGenericRepository<Domain.Entities.Models.Clients.Services, ServicesFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
