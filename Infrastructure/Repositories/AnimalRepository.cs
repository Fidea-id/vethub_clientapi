using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class AnimalRepository : GenericRepository<Animals, NameBaseEntityFilter>, IAnimalRepository
    {
        public AnimalRepository(IDBFactory context) : base(context)
        {
        }
    }
}
