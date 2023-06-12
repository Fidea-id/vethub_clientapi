using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Interfaces;
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

