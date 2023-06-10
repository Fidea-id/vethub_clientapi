using Application.Services.Contracts;
using Domain.Entities.FIlters;
using Domain.Entities.Models;
using Domain.Entities.Requests;
using Domain.Interfaces;

namespace Application.Services.Implementations
{
    public class OwnersService : GenericService<Owners, OwnersRequest, Owners, OwnersFilter>, IOwnersService
    {
        public OwnersService(IUnitOfWork unitOfWork, IGenericRepository<Owners, OwnersFilter> repository)
        : base(unitOfWork, repository)
        { }
    }
}
