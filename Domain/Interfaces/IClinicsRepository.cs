using Domain.Entities.Filters;
using Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IClinicsRepository : IGenericRepository<Clinics, ClinicsFilter>
    {
    }
}
