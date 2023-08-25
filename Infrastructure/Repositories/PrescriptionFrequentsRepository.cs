using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PrescriptionFrequentsRepository : GenericRepository<PrescriptionFrequents, PrescriptionFrequentsFilter>, IPrescriptionFrequentsRepository
    {
        public PrescriptionFrequentsRepository(IDBFactory context) : base(context)
        {
        }
    }
}
