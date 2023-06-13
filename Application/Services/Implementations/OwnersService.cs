using Application.Services.Contracts;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests;
using Domain.Interfaces;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class OwnersService : GenericService<Owners, OwnersRequest, Owners, OwnersFilter>, IOwnersService
    {
        public OwnersService(IUnitOfWork unitOfWork, IGenericRepository<Owners, OwnersFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
