using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OwnersRepository : GenericRepository<Owners, OwnersFilter>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }

		public async Task<Owners> ReadByPatientIdAsync(int id, string dbName)
		{
			var _db = _dbFactory.GetDbConnection(dbName);
			return await _db.QueryFirstOrDefaultAsync<Owners>($"SELECT O.* FROM Owners O JOIN Patients P ON P.OwnersId = O.Id WHERE P.IsActive = true AND P.Id = {id}");
		}
	}
}
