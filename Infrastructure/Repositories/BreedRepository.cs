using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class BreedRepository : GenericRepository<Breeds, NameBaseEntityFilter>, IBreedRepository
    {
        public BreedRepository(IDBFactory context) : base(context)
        {
        }
    }
}
