using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OwnersRepository : GenericRepository<Owners, OwnersFilter>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetOwnerChart(string dbName, string dateFilter)
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
                                Owners
                            WHERE 
            ";
            query += filterQuery;
            query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }

        public async Task<Owners> ReadByPatientIdAsync(int id, string dbName)
		{
			var _db = _dbFactory.GetDbConnection(dbName);
			return await _db.QueryFirstOrDefaultAsync<Owners>($"SELECT O.* FROM Owners O JOIN Patients P ON P.OwnersId = O.Id WHERE P.IsActive = true AND P.Id = {id}");
		}

    }
}
