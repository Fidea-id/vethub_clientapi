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

        public async Task<DashboardOrderResponse> GetOrdersDashboard(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                string query = @"SELECT
                COUNT(CASE WHEN Type = 'Incomes' THEN 1 END) AS IncomesTotal,
                COUNT(CASE WHEN Type = 'Expenses' THEN 1 END) AS ExpensesTotal,
                SUM(CASE WHEN Type = 'Incomes' AND Status = 'Paid' THEN TotalPrice ELSE 0 END) AS IncomesAmount,
                SUM(CASE WHEN Type = 'Expenses' AND Status = 'Paid' THEN TotalPrice ELSE 0 END) AS ExpensesAmount
            FROM Orders
            WHERE MONTH(Date) = MONTH(CURRENT_DATE) AND IsActive = 1";
                return await _db.QueryFirstOrDefaultAsync<DashboardOrderResponse>(query);
            }
        }
        public async Task<string> GetLatestCode(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                string query = "SELECT OrderNumber FROM Orders Where IsActive = 1 ORDER BY Id DESC";
                return await _db.QueryFirstOrDefaultAsync<string>(query);
            }
        }
        public async Task<DataResultDTO<OrdersResponse>> GetOrdersList(string dbName, OrdersFilter filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
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
        }

        public async Task<DataResultDTO<OrderFullResponse>> GetListOrderFull(string dbName, OrderFilterRequest filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var conditions = new List<string>();
                var parameters = new DynamicParameters();

                conditions.Add("o.IsActive = 1");

                if (filter.Month.HasValue)
                {
                    conditions.Add("MONTH(o.Date) = @Month");
                    parameters.Add("Month", filter.Month.Value);
                }
                if (filter.Year.HasValue)
                {
                    conditions.Add("YEAR(o.Date) = @Year");
                    parameters.Add("Year", filter.Year.Value);
                }
                if (!string.IsNullOrEmpty(filter.Type))
                {
                    conditions.Add("o.Type = @Type");
                    parameters.Add("Type", filter.Type);
                }
                if (!string.IsNullOrEmpty(filter.Status))
                {
                    conditions.Add("o.Status = @Status");
                    parameters.Add("Status", filter.Status);
                }
                if (filter.MinPrice.HasValue)
                {
                    conditions.Add("o.TotalPrice >= @MinPrice");
                    parameters.Add("MinPrice", filter.MinPrice.Value);
                }
                if (filter.MaxPrice.HasValue)
                {
                    conditions.Add("o.TotalPrice <= @MaxPrice");
                    parameters.Add("MaxPrice", filter.MaxPrice.Value);
                }

                string whereClause = conditions.Count > 0 ? " WHERE " + string.Join(" AND ", conditions) : "";

                string countQuery = @"
                    SELECT COUNT(1)
                    FROM Orders o
                    LEFT JOIN Profile p ON o.StaffId = p.Id
                    LEFT JOIN Owners c ON o.ClientId = c.Id" + whereClause;

                int totalCount = await _db.ExecuteScalarAsync<int>(countQuery, parameters);

                int offset = (filter.PageNumber - 1) * filter.PageSize;

                string query = $@"
                SELECT
                    o.Id AS Id,
                    o.OrderNumber,
                    o.Date,
                    o.DueDate,
                    o.ClientId,
                    c.Name AS ClientName,
                    o.StaffId,
                    p.Name AS StaffName,
                    o.Type,
                    o.TotalQuantity,
                    o.Status,
                    o.TotalPrice,
                    o.TotalDiscountedPrice,
                    o.TotalDiscount
                FROM Orders o
                LEFT JOIN Profile p ON o.StaffId = p.Id
                LEFT JOIN Owners c ON o.ClientId = c.Id
                {whereClause}
                ORDER BY o.Id DESC
                LIMIT @Limit OFFSET @Offset;";

                parameters.Add("Limit", filter.PageSize);
                parameters.Add("Offset", offset);

                var results = (await _db.QueryAsync<OrderFullResponse>(query, parameters)).ToList();

                var clinicData = new ClientClinicResponse();
                foreach (var item in results)
                {
                    const string productsQuery = @"
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
                    WHERE o.Id = @OrderId AND o.IsActive = 1";
                    item.OrderProducts = await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = item.Id });

                    const string paymentQuery = @"
                    SELECT
                        o.Id AS OrderId,
                        op.PaymentMethodId,
                        pm.Name,
                        op.Date,
                        op.Total,
                        op.Status
                    FROM Orders o
                    JOIN OrdersPayment op ON o.Id = op.OrderId
                    JOIN PaymentMethod pm ON pm.Id = op.PaymentMethodId
                    WHERE o.Id = @OrderId AND op.Type = @PaymentType AND o.IsActive = 1";
                    item.OrderPayments = await _db.QueryAsync<OrdersPaymentResponse>(paymentQuery, new { OrderId = item.Id, PaymentType = "Order" });

                    item.ClinicData = clinicData;
                }

                var result = new DataResultDTO<OrderFullResponse>
                {
                    Data = results,
                    TotalData = totalCount
                };
                return result;
            }
        }

        public async Task<IEnumerable<OrderFullResponse>> GetListOrderFull(string dbName, bool thisMonth = false)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {

                string query = @"
                SELECT
                    o.Id AS Id,
                    o.OrderNumber,
                    o.Date,
                    o.DueDate,
                    o.ClientId,
                    c.Name AS ClientName,
                    o.StaffId,
                    p.Name AS StaffName,
                    o.Type,
                    o.TotalQuantity,
                    o.Status,
                    o.TotalPrice,
                    o.TotalDiscountedPrice,
                    o.TotalDiscount
                FROM Orders o
                LEFT JOIN Profile p ON o.StaffId = p.Id
                LEFT JOIN Owners c ON o.ClientId = c.Id";
                if (thisMonth)
                {
                    query += " WHERE MONTH(o.Date) = MONTH(CURRENT_DATE()) AND YEAR(o.Date) = YEAR(CURRENT_DATE()) AND o.IsActive = 1;";
                }
                else
                {
                    query += " WHERE o.IsActive = 1;";
                }
                var results = await _db.QueryAsync<OrderFullResponse>(query);

                var clinicData = new ClientClinicResponse();
                foreach (var item in results)
                {
                    const string productsQuery = @"
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
                    WHERE o.Id = @OrderId AND o.IsActive = 1";
                    item.OrderProducts = await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = item.Id });

                    const string paymentQuery = @"
                    SELECT
                        o.Id AS OrderId,
                        op.PaymentMethodId,
                        pm.Name,
                        op.Date,
                        op.Total,
                        op.Status
                    FROM Orders o
                    JOIN OrdersPayment op ON o.Id = op.OrderId
                    JOIN PaymentMethod pm ON pm.Id = op.PaymentMethodId
                    WHERE o.Id = @OrderId AND op.Type = @PaymentType AND o.IsActive = 1";
                    item.OrderPayments = await _db.QueryAsync<OrdersPaymentResponse>(paymentQuery, new { OrderId = item.Id, PaymentType = "Order" });

                    item.ClinicData = clinicData;
                }
                return results;
            }
        }

        public async Task<OrderFullResponse> GetOrderFull(string dbName, int id)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
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
                    o.TotalQuantity,
                    o.Type,
                    o.Status,
                    o.TotalPrice,
                    o.TotalDiscountedPrice,
                    o.TotalDiscount
                FROM Orders o
                LEFT JOIN Profile p ON o.StaffId = p.Id
                LEFT JOIN Owners c ON o.ClientId = c.Id
                WHERE o.Id = @OrderId AND o.IsActive = 1";
                var results = await _db.QueryFirstAsync<OrderFullResponse>(query, new { OrderId = id });

                const string clinicQuery = @"
                SELECT 
                    Id,
                    Name,
                    Email,
                    Logo,
                    Address,
                    City,
                    State,
                    Description,
                    PhoneNumber,
                    WebUrl,
                    MapUrl
                FROM
                    Clinics";
                var clinicData = await _db.QueryFirstAsync<ClientClinicResponse>(clinicQuery);

                const string productsQuery = @"
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
                WHERE o.Id = @OrderId AND o.IsActive = 1";
                results.OrderProducts = await _db.QueryAsync<OrdersDetailResponse>(productsQuery, new { OrderId = id });

                const string paymentQuery = @"
                SELECT
                    o.Id AS OrderId,
                    op.PaymentMethodId,
                    pm.Name,
                    op.Date,
                    op.Total,
                    op.Status
                FROM Orders o
                JOIN OrdersPayment op ON o.Id = op.OrderId
                JOIN PaymentMethod pm ON pm.Id = op.PaymentMethodId
                WHERE o.Id = @OrderId AND op.Type = @PaymentType AND o.IsActive = 1";
                results.OrderPayments = await _db.QueryAsync<OrdersPaymentResponse>(paymentQuery, new { OrderId = id, PaymentType = "Order" });
                results.ClinicData = clinicData;
                return results;
            }
        }

        public async Task<CardDashboard> CountInvoiceToCard(string dbName, string query)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var filter = "";
                if (!string.IsNullOrEmpty(query))
                {
                    filter = $"And {query}";
                }

                var queryOrder = $"SELECT COUNT(Id) AS `TotalAll`, COUNT(Id) AS `Total` FROM Orders WHERE `Status` = 'Paid' AND `IsActive` = 1 AND `Type` = 'Incomes' {filter}";
                var queryAppointment = $"SELECT COUNT(Id) AS `TotalAll`, COUNT(Id) AS `Total` FROM Appointments WHERE `StatusId` = 6 AND `IsActive` = 1 {filter}";

                var resultQuery = $"SELECT SUM(TotalAll) AS `TotalAll`, SUM(Total) AS `Total` FROM ({queryOrder} UNION ALL {queryAppointment}) AS CombinedCounts;";
                return await _db.QueryFirstAsync<CardDashboard>(resultQuery);
            }
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetTotalOrderSales(string dbName, string dateFilter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var filterQuery = "YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE()";
                if (dateFilter != null)
                {
                    filterQuery = dateFilter;
                }
                string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%d') AS Date,
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                SUM(TotalPrice) AS Total
                            FROM
                                Orders
                            WHERE Type = 'Incomes' AND `Status` = 'Paid' AND `IsActive` = 1 AND 
            ";
                query += filterQuery;
                query += " GROUP BY DATE_FORMAT(CreatedAt, '%d'), DATE_FORMAT(CreatedAt, '%m'), DATE_FORMAT(CreatedAt, '%Y') ORDER BY Date, Year, Month;";
                return await _db.QueryAsync<MonthlyDataChart>(query);
            }
        }
    }
}
