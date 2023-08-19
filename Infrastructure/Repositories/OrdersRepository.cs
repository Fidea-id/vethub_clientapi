using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Entities.Responses.Masters;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;

namespace Infrastructure.Repositories
{
    public class OrdersRepository : GenericRepository<Orders, OrdersFilter>, IOrdersRepository
    {
        public OrdersRepository(IDBFactory context) : base(context)
        {
        }
        public async Task<DataResultDTO<OrdersResponse>> GetOrdersList(string dbName, OrdersFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var mainTableName = "Orders";
            var joinQuery = "LEFT JOIN Owners ON Orders.ClientId = Owners.Id LEFT JOIN Profile ON Profile.Id = Orders.StaffId";
            var selectColumns = new List<string> { "Orders.*", "Owners.Name as ClientName", "Profile.Name as StaffName" };
            var filterQuery = QueryGenerator.GenerateFilterQuery(filter, mainTableName, joinQuery, selectColumns);
            var queryString = filterQuery.Item1;
            var countQuery = QueryGenerator.GenerateSelectOrCountQuery(filterQuery.Item1, true);
            var countData = await _db.QueryFirstOrDefaultAsync<int>(countQuery, filterQuery.Item2);
            if (filter.Take.HasValue || filter.Skip.HasValue)
            {
                queryString = QueryGenerator.GenerateFilteredLimitQuery(queryString, filter.Skip, filter.Take);
            }
            var data = await _db.QueryAsync<OrdersResponse>(queryString, filterQuery.Item2);
            var result = new DataResultDTO<OrdersResponse>
            {
                Data = data,
                TotalData = countData
            };
            return result;
        }

        public async Task<IEnumerable<OrderFullResponse>> GetListOrderFull(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            const string query = @"
                SELECT
                    o.Id AS Id,
                    o.OrderNumber,
                    o.Date,
                    o.DueDate,
                    o.ClientId,
                    c.Name AS ClientName,
                    o.StaffId,
                    p.Name AS StaffName,
                    o.Quantity,
                    o.Status,
                    o.Discount,
                    o.DiscountType,
                    o.TotalPrice,
                    o.TotalDiscountedPrice
                FROM Orders o
                LEFT JOIN Profile p ON o.StaffId = p.Id
                LEFT JOIN Owners c ON o.ClientId = c.Id";
            var results = await _db.QueryAsync<OrderFullResponse>(query);

            //TODO: Clinic Data
            var clinicData = new ClientClinicResponse();
            foreach (var item in results)
            {
                const string productsQuery = @"
                    SELECT
                        od.ProductId,
                        pr.Name AS ProductName,
                        od.Quantity AS ProductQuantity,
                        pr.Price AS ProductPrice,
                        od.Discount AS ProductDiscount,
                        od.DiscountType AS ProductDiscountType,
                        od.TotalPrice AS ProductTotalPrice
                    FROM Orders o
                    LEFT JOIN OrdersDetail od ON o.Id = od.OrderId
                    LEFT JOIN Products pr ON od.ProductId = pr.Id
                    WHERE o.Id = @OrderId";
                item.OrderProducts = await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = item.Id });

                const string paymentQuery = @"
                    SELECT
                        op.PaymentMethodId,
                        pm.Name,
                        op.Date,
                        op.Total,
                        op.Status
                    FROM Orders o
                    JOIN OrdersPayment op ON o.Id = op.OrderId
                    JOIN PaymentMethod pm ON pm.Id = op.PaymentMethodId
                    WHERE o.Id = @OrderId";
                item.OrderPayments = await _db.QueryAsync<OrdersPaymentResponse>(paymentQuery, new { OrderId = item.Id });

                item.ClinicData = clinicData;
            }
            return results;
        }

        public async Task<OrderFullResponse> GetOrderFull(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            const string query = @"
                SELECT
                    o.Id AS Id,
                    o.OrderNumber,
                    o.Date,
                    o.DueDate,
                    o.ClientId,
                    c.Name AS ClientName,
                    o.StaffId,
                    p.Name AS StaffName,
                    o.Quantity,
                    o.Status,
                    o.Discount,
                    o.DiscountType,
                    o.TotalPrice,
                    o.TotalDiscountedPrice
                FROM Orders o
                LEFT JOIN Profile p ON o.StaffId = p.Id
                LEFT JOIN Owners c ON o.ClientId = c.Id
                WHERE o.Id = @OrderId";
            var results = await _db.QueryFirstAsync<OrderFullResponse>(query, new { OrderId = id });

            //TODO: Clinic Data
            var clinicData = new ClientClinicResponse();
            const string productsQuery = @"
                SELECT
                    od.ProductId,
                    pr.Name AS ProductName,
                    od.Quantity AS ProductQuantity,
                    pr.Price AS ProductPrice,
                    od.Discount AS ProductDiscount,
                    od.DiscountType AS ProductDiscountType,
                    od.TotalPrice AS ProductTotalPrice
                FROM Orders o
                LEFT JOIN OrdersDetail od ON o.Id = od.OrderId
                LEFT JOIN Products pr ON od.ProductId = pr.Id
                WHERE o.Id = @OrderId";
            results.OrderProducts = await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = id });

            const string paymentQuery = @"
                SELECT
                    op.PaymentMethodId,
                    pm.Name,
                    op.Date,
                    op.Total,
                    op.Status
                FROM Orders o
                JOIN OrdersPayment op ON o.Id = op.OrderId
                JOIN PaymentMethod pm ON pm.Id = op.PaymentMethodId
                WHERE o.Id = @OrderId";
            results.OrderPayments = await _db.QueryAsync<OrdersPaymentResponse>(paymentQuery, new { OrderId = id });
            results.ClinicData = clinicData;
            return results;
        }
    }
}
