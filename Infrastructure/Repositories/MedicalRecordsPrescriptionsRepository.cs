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
    }
}
