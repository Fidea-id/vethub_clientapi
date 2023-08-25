using Dapper;
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

        public async Task<Profile> GetByGlobalId(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstOrDefaultAsync<Profile>($"SELECT * FROM {_tableName} WHERE GlobalId = @Id", new { Id = id });
        }
        public async Task<Profile> GetByEmail(string dbName, string email)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstOrDefaultAsync<Profile>($"SELECT * FROM {_tableName} WHERE Email = @Email", new { Email = email });
        }
    }
}

