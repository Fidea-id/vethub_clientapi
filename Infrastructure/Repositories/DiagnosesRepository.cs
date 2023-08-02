using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class DiagnosesRepository : GenericRepository<Diagnoses, NameBaseEntityFilter>, IDiagnosesRepository
    {
        public DiagnosesRepository(IDBFactory context) : base(context)
        {
        }
    }
}
