using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class DiagnosesRepository : GenericRepository<Diagnoses, NameBaseEntityFilter>, IDiagnosesRepository
    {
        public DiagnosesRepository(IDBFactory context) : base(context)
        {
        }
    }
}
