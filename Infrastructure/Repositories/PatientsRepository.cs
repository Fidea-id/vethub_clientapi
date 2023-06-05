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
    public class PatientsRepository : GenericRepository<Patients>, IPatientsRepository
    {
        public PatientsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
