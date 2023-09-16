using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OrdersPaymentRepository : GenericRepository<OrdersPayment, OrdersPaymentFilter>, IOrdersPaymentRepository
    {
        public OrdersPaymentRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<OrdersPayment>> GetPaidByOrderId(string dbName, int orderId, string type)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string query = @"SELECT * FROM OrdersPayment WHERE OrderId = @orderId AND Type = @type AND Status = @status ";
            return await _db.QueryAsync<OrdersPayment>(query, new { orderId = orderId, type = type, status = "Paid" });
        }
    }
}
