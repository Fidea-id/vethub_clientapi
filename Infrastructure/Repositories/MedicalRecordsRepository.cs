using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
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
            string query = "SELECT Code FROM MedicalRecords ORDER BY Id DESC";
            return await _db.QueryFirstOrDefaultAsync<string>(query);
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetVisitYearly(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                COUNT(*) AS Total
                            FROM
                                MedicalRecords
                            WHERE
                                YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE()
                            GROUP BY
                                Month, Year
                            ORDER BY
                                Year, Month;
            ";
            return await _db.QueryAsync<MonthlyDataChart>(query);
        }
    }
}
