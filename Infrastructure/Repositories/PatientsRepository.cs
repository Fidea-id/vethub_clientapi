using Domain.Entities.Models;
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
