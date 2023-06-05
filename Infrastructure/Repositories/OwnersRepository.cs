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
    internal class OwnersRepository : GenericRepository<Owners>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }
    }
}
