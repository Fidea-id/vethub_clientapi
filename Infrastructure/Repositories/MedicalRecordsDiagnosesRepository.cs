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
    }
}
