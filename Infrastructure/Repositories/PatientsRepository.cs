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
        public async Task<IEnumerable<MonthlyDataChart>> GetPatientChart(string dbName, string dateFilter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filterQuery = "YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE()";
            if (dateFilter != null)
            {
                filterQuery = dateFilter;
            }
            string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%d') AS Date,
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                COUNT(*) AS Total
                            FROM
                                Patients
                            WHERE 
            ";
            query += filterQuery;
            query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }
        public async Task<IEnumerable<MonthlyDataChart>> GetPatientTypeChart(string dbName, string dateFilter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filterQuery = "YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE()";
            if (dateFilter != null)
            {
                filterQuery = dateFilter;
            }
            string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%d') AS Date,
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                Species AS Type,
                                COUNT(*) AS Total
                            FROM
                                Patients
                            WHERE 
            ";
            query += filterQuery;
            query += " GROUP BY Species, Date, Month, Year ORDER BY Date, Year, Month;";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }
    }
}
