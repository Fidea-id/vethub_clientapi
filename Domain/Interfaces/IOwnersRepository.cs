using Domain.Entities.FIlters;

namespace Domain.Interfaces
{
    public interface IOwnersRepository : IGenericRepository<Owners, OwnersFilter>
    {
    }
}
