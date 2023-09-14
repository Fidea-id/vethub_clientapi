using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsPrescriptionsRepository : GenericRepository<MedicalRecordsPrescriptions, MedicalRecordsPrescriptionsFilter>, IMedicalRecordsPrescriptionsRepository
    {
        public MedicalRecordsPrescriptionsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicalRecordsPrescriptions>> GetByMedicalRecordId(string dbName, int medicalRecordsId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<MedicalRecordsPrescriptions>($"SELECT * FROM MedicalRecordsPrescriptions WHERE MedicalRecordsId = @Id", new { Id = medicalRecordsId });
        }
    }
}
