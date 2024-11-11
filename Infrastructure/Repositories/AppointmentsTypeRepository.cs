using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class AppointmentsTypeRepository : GenericRepository<AppointmentsType, NameBaseEntityFilter>, IAppointmentsTypeRepository
    {
        public AppointmentsTypeRepository(IDBFactory context) : base(context)
        {
        }
    }
}
