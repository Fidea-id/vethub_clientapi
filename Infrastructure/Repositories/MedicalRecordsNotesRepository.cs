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
    }
}
