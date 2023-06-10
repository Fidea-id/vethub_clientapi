using Domain.Entities.FIlters;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProfileRepository : GenericRepository<Services, ServicesFilter>, IProfileRepository
    {
        public ProfileRepository(IDBFactory context) : base(context)
        {
        }
    }
}

