using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsNotesRepository : GenericRepository<MedicalRecordsNotes, MedicalRecordsNotesFilter>, IMedicalRecordsNotesRepository
    {
        public MedicalRecordsNotesRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<MedicalRecordsNotes> CheckRecordType(string dbName, int medicalRecordsId, string type)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstOrDefaultAsync<MedicalRecordsNotes>($"SELECT * FROM MedicalRecordsNotes WHERE MedicalRecordsId = @Id And Type = @NoteType", new { Id = medicalRecordsId, NoteType = type });
        }

        public async Task<IEnumerable<MedicalRecordsNotes>> GetByMedicalRecordId(string dbName, int medicalRecordsId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<MedicalRecordsNotes>($"SELECT * FROM MedicalRecordsNotes WHERE MedicalRecordsId = @Id", new { Id = medicalRecordsId });
        }
    }
}
