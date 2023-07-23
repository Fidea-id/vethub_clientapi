using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    internal class AnimalRepository : GenericRepository<Animals, NameBaseEntityFilter>, IAnimalRepository
    {
        public AnimalRepository(IDBFactory context) : base(context)
        {
        }
    }
}
