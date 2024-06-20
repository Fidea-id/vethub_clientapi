using Domain.Entities;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    internal class ProductStockHistoricalRepository : GenericRepository<ProductStockHistorical, BaseEntityFilter>, IProductStockHistoricalRepository
    {
        public ProductStockHistoricalRepository(IDBFactory context) : base(context)
        {
        }
    }
}
