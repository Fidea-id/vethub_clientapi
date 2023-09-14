using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsDiagnosesRepository : GenericRepository<MedicalRecordsDiagnoses, MedicalRecordsDiagnosesFilter>, IMedicalRecordsDiagnosesRepository
    {
        public MedicalRecordsDiagnosesRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicalRecordsDiagnoses>> GetByMedicalRecordId(string dbName, int medicalRecordsId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<MedicalRecordsDiagnoses>($"SELECT * FROM MedicalRecordsDiagnoses WHERE MedicalRecordsId = @Id", new { Id = medicalRecordsId });
        }
    }
}
