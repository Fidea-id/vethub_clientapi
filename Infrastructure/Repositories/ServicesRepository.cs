using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ServicesRepository : GenericRepository<Services, ServicesFilter>, IServicesRepository
    {
        public ServicesRepository(IDBFactory context) : base(context)
        {
        }
    }
}
