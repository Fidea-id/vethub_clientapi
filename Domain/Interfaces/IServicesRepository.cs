using Domain.Entities.FIlters;

namespace Domain.Interfaces
{
    public interface IServicesRepository : IGenericRepository<Services, ServicesFilter>
    {
    }
}
