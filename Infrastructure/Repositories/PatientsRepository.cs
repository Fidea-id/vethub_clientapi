using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;

namespace Infrastructure.Repositories
{
    public class PatientsRepository : GenericRepository<Patients, PatientsFilter>, IPatientsRepository
    {
        public PatientsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<PatientsListResponse>> GetPatientsList(string dbName, PatientsFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var mainTableName = "Patients";
            var joinQuery = "JOIN Owners ON Patients.OwnersId = Owners.Id";
            var selectColumns = new List<string> { "Patients.*", "Owners.Name as OwnersName" };
            var result = QueryGenerator.GenerateFilterQuery(filter, mainTableName, joinQuery, selectColumns);
            return await _db.QueryAsync<PatientsListResponse>(result.Item1, result.Item2);
        }
    }
}
