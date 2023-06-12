using Domain.Entities.Filters;
using Domain.Entities.Models;

namespace Domain.Interfaces
{
    public interface IServicesRepository : IGenericRepository<Services, ServicesFilter>
    {
    }
}
