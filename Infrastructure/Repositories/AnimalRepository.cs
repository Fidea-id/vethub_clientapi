using Dapper;
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

        public async Task<Animals> GetByName(string dbName, string name)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstOrDefaultAsync<Animals>($"SELECT * FROM Animals WHERE Name = @Name", new { Name = name });
        }
    }
}
