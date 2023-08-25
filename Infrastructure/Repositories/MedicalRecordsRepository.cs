using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsRepository : GenericRepository<MedicalRecords, MedicalRecordsFilter>, IMedicalRecordsRepository
    {
        public MedicalRecordsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<string> GetLatestCode(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string query = "SELECT TOP 1 Code FROM MedicalRecords ORDER BY Id DESC";
            return await _db.QueryFirstOrDefaultAsync<string>(query);

        }
    }
}
