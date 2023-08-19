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
                    a.PatientId,
                    MAX(a.StaffId) AS StaffId,
                    a.Type,
                    MAX(a.Unit) AS Unit,
                    MAX(a.CreatedAt) AS CreatedAt,
                    (SELECT MAX(Value) FROM PatientsStatistic c WHERE c.PatientId = a.PatientId AND c.Type = a.Type AND c.CreatedAt = MAX(a.CreatedAt)) AS Latest,
                    (SELECT Value FROM PatientsStatistic d WHERE d.PatientId = a.PatientId AND d.Type = a.Type AND d.CreatedAt = (SELECT MAX(CreatedAt) 
                        FROM PatientsStatistic e WHERE e.PatientId = a.PatientId AND e.Type = a.Type AND e.CreatedAt < MAX(a.CreatedAt))) AS `Before`
                FROM PatientsStatistic a
                WHERE a.PatientId = @patientId
                GROUP BY a.PatientId, a.Type;";

            return await _db.QueryAsync<PatientsStatisticDto>(query, new { patientId });
        }
    }
}
