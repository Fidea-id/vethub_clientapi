using Domain.Entities.Filters;
using Domain.Entities.Models;

namespace Domain.Interfaces
{
    public interface IOwnersRepository : IGenericRepository<Owners, OwnersFilter>
    {
    }
}
