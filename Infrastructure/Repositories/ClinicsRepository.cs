using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ClinicsRepository : GenericRepository<Clinics, ClinicsFilter>, IClinicsRepository
    {
        public ClinicsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
