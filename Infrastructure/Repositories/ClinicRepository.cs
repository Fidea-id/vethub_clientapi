using Domain.Entities.Filters;
using Domain.Entities.Filters.Clients;
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
    public class ClinicRepository : GenericRepository<Clinics, ClinicsFilter>, IClinicRepository
    {
        public ClinicRepository(IDBFactory context) : base(context)
        {
        }
    }
}
