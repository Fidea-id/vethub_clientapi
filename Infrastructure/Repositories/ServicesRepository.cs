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
    public class ServicesRepository : GenericRepository<Services>, IServicesRepository
    {
        public ServicesRepository(IDBFactory context) : base(context)
        {
        }
    }
}
