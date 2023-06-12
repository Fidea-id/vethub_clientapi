using Application.Services.Contracts;
using Domain.Entities.Filters;
using Domain.Entities.Requests;
using Domain.Interfaces;

namespace Application.Services.Implementations
{
    public class ServicesService : GenericService<Domain.Entities.Models.Services, ServicesRequest, Domain.Entities.Models.Services, ServicesFilter>, IServicesService
    {
        public ServicesService(IUnitOfWork unitOfWork, IGenericRepository<Domain.Entities.Models.Services, ServicesFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
