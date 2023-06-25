using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using static Dapper.SqlMapper;

namespace Infrastructure.Repositories
{
    public class OwnersRepository : GenericRepository<Owners, OwnersFilter>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }
    }
}
