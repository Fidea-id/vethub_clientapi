using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductDiscountsRepository : GenericRepository<ProductDiscounts, ProductDiscountsFilter>, IProductDiscountsRepository
    {
        protected readonly string _tableDiscountName;
        public ProductDiscountsRepository(IDBFactory context) : base(context)
        {
            _tableDiscountName = typeof(ProductDiscounts).Name;
        }

        public Task<IEnumerable<ProductDiscounts>> GetProductDiscountByProduct(int productId, string dbName)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProductDiscountDetailResponse>> GetProductDiscountDetail(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
                SELECT
                    pd.Id,
                    pd.ProductId,
                    p.Name AS ProductName,
                    pd.StartDate,
                    pd.EndDate,
                    pd.IsActive,
                    pd.DiscountValue,
                    pd.DiscountType,
                    p.Price,
                    CASE
                    WHEN pd.DiscountType = 'Amount' THEN p.Price - pd.DiscountValue
                    WHEN pd.DiscountType = 'Percentage' THEN p.Price - (p.Price * pd.DiscountValue / 100)
                    ELSE p.Price
                    END AS DiscountedPrice
                FROM
                    ProductDiscounts pd
                JOIN
                    Products p ON pd.ProductId = p.Id";
            return await _db.QueryAsync<ProductDiscountDetailResponse>(query);
        }
    }
}
