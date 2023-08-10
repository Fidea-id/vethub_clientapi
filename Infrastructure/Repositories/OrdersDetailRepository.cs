using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OrdersDetailRepository : GenericRepository<OrdersDetail, OrdersDetailFilter>, IOrdersDetailRepository
    {
        public OrdersDetailRepository(IDBFactory context) : base(context)
        {
        }
    }
}
