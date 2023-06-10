using Domain.Entities.FIlters;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OwnersRepository : GenericRepository<Owners, OwnersFilter>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }
    }
}
