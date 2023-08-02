using Dapper;
using Domain.Entities.DTOs;
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

        public async Task<IEnumerable<Patients>> GetPatientsByOwner(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<Patients>($"SELECT * FROM Patients where IsActive = true AND OwnersId = {id}");
        }

        public async Task<DataResultDTO<PatientsListResponse>> GetPatientsList(string dbName, PatientsFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var mainTableName = "Patients";
            var joinQuery = "JOIN Owners ON Patients.OwnersId = Owners.Id";
            var selectColumns = new List<string> { "Patients.*", "Owners.Name as OwnersName" };
            var filterQuery = QueryGenerator.GenerateFilterQuery(filter, mainTableName, joinQuery, selectColumns);
            var queryString = filterQuery.Item1;
            var countQuery = QueryGenerator.GenerateSelectOrCountQuery(filterQuery.Item1, true);
            var countData = await _db.QueryFirstOrDefaultAsync<int>(countQuery, filterQuery.Item2);
            if (filter.Take.HasValue || filter.Skip.HasValue)
            {
                queryString = QueryGenerator.GenerateFilteredLimitQuery(queryString, filter.Skip, filter.Take);
            }
            var data = await _db.QueryAsync<PatientsListResponse>(queryString, filterQuery.Item2);
            var result = new DataResultDTO<PatientsListResponse>
            {
                Data = data,
                TotalData = countData
            };
            return result;
        }
    }
}
