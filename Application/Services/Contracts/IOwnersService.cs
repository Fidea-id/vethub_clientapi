using Domain.Entities.FIlters;
using Domain.Entities.Models;
using Domain.Entities.Requests;

namespace Application.Services.Contracts
{
    public interface IOwnersService : IGenericService<Owners, OwnersRequest, Owners, OwnersFilter>
    {
    }
}
