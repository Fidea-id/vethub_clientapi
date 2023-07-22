using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
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
