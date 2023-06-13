using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class PatientsRepository : GenericRepository<Patients, PatientsFilter>, IPatientsRepository
    {
        public PatientsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
