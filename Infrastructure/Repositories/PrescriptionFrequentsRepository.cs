using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class PrescriptionFrequentsRepository : GenericRepository<PrescriptionFrequents, PrescriptionFrequentsFilter>, IPrescriptionFrequentsRepository
    {
        public PrescriptionFrequentsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
