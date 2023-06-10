using Domain.Entities.FIlters;

namespace Domain.Interfaces
{
    public interface IProfileRepository : IGenericRepository<Services, ServicesFilter>
    {
    }
}

