using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ClinicsRepository : GenericRepository<Clinics, ClinicsFilter>, IClinicsRepository
    {
        public ClinicsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
