using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OpnamesRepository : GenericRepository<Opnames, OpnamesFilter>, IOpnamesRepository
    {
        public OpnamesRepository(IDBFactory context) : base(context)
        {
        }
    }
}
