using Dapper;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class PatientsStatisticRepository : GenericRepository<PatientsStatistic, PatientsStatisticFilter>, IPatientsStatisticRepository
    {
        public PatientsStatisticRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<PatientsStatisticDto>> ReadPatientsStatisticAsync(int patientId, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            string query = $@"
                SELECT
                    a.PatientId, MAX(a.StaffId) AS StaffId, a.Type, MAX(a.Unit) AS Unit, MAX(a.CreatedAt) AS CreatedAt, MAX(a.Value) AS Latest, MAX(b.Value) AS `Before`
                FROM PatientsStatistic a
                LEFT JOIN (
                    SELECT
                        PatientId, Value, CreatedAt
                    FROM PatientsStatistic
                    WHERE PatientId = @patientId AND CreatedAt < (
                        SELECT MAX(CreatedAt)
                        FROM PatientsStatistic
                        WHERE PatientId = @patientId
                    )
                ) b ON a.PatientId = b.PatientId
                WHERE a.PatientId = @patientId
                GROUP BY a.Type
                ORDER BY MAX(a.CreatedAt) DESC;";

            return await _db.QueryAsync<PatientsStatisticDto>(query, new { patientId });
        }
    }
}
