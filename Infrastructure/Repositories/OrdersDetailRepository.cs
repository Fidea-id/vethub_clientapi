using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OrdersDetailRepository : GenericRepository<OrdersDetail, OrdersDetailFilter>, IOrdersDetailRepository
    {
        public OrdersDetailRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<OrdersDetailResponse>> GetByOrderId(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string productsQuery = @"
                    SELECT
                     od.ProductId,
                     pr.Name AS ProductName,
                     od.Quantity AS Quantity,
                     pr.Price AS Price,
                     od.Discount AS Discount,
                     od.DiscountType AS DiscountType,
                     od.TotalPrice AS TotalPrice
                    FROM Orders o
                    LEFT JOIN OrdersDetail od ON o.Id = od.OrderId
                    LEFT JOIN Products pr ON od.ProductId = pr.Id
                    WHERE o.Id = @OrderId";
            return await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = id });
        }
    }
}
