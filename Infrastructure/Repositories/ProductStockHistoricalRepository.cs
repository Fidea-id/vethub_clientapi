using Domain.Entities;
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
    internal class ProductStockHistoricalRepository : GenericRepository<ProductStockHistorical, BaseEntityFilter>, IProductStockHistoricalRepository
    {
        public ProductStockHistoricalRepository(IDBFactory context) : base(context)
        {
        }
    }
}
