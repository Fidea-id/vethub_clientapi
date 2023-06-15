using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProfileRepository : GenericRepository<Profile, ProfileFilter>, IProfileRepository
    {
        public ProfileRepository(IDBFactory context) : base(context)
        {
        }
    }
}

