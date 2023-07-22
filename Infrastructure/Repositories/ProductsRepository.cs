using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using static Dapper.SqlMapper;

namespace Infrastructure.Repositories
{
    public class ProductsRepository : GenericRepository<Products, ProductsFilter>, IProductsRepository
    {
        public ProductsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<ProductDetailsResponse> GetProductDetails(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
            SELECT
                p.Id,
                p.Name,
                p.Description,
                ps.Stock,
                ps.Volume,
                pc.Name AS Category,
                IF(COUNT(pb.BundleId) > 0, 1, 0) AS IsBundle,
                IF(COUNT(pd.DiscountId) > 0, 1, 0) AS HasDiscount
            FROM
                {dbName}.Products p
                INNER JOIN {dbName}.ProductStocks ps ON p.Id = ps.ProductId
                INNER JOIN {dbName}.ProductCategories pc ON p.CategoryId = pc.Id
                LEFT JOIN {dbName}.ProductBundles pb ON p.Id = pb.ProductId
                LEFT JOIN {dbName}.ProductDiscounts pd ON p.Id = pd.ProductId
            WHERE
                p.Id = @ProductId
            GROUP BY
                p.Id"
            ;
            var result = await _db.QueryFirstOrDefaultAsync<ProductDetailsResponse>(query, new { ProductId = id });
            if (result == null)
                return null;

            if (result.IsBundle)
            {
                const string bundlesQuery = @"
                SELECT
                    pb.BundleId AS Id,
                    pb.BundledProductId AS ProductId,
                    pb.Quantity
                FROM
                    {dbName}.ProductBundles pb
                WHERE
                    pb.ProductId = @ProductId"
                ;
                result.Bundles = await _db.QueryAsync<ProductBundles>(bundlesQuery, new { ProductId = id });
            }

            if (result.HasDiscount)
            {
                const string discountsQuery = @"
                SELECT
                    pd.DiscountId AS Id,
                    pd.ProductId,
                    pd.DiscountValue,
                    pd.DiscountType
                FROM
                    {dbName}.ProductDiscounts pd
                WHERE
                    pd.ProductId = @ProductId"
                ;
                result.Discounts = await _db.QueryAsync<ProductDiscounts>(discountsQuery, new { ProductId = id });
            }
            return result;
        }
        public async Task<IEnumerable<ProductDetailsResponse>> GetListProductDetails(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
            SELECT
                p.Id,
                p.Name,
                p.Description,
                ps.Stock,
                ps.Volume,
                p.Price,
                pc.Name AS Category,
                IF(COUNT(pb.BundleId) > 0, 1, 0) AS IsBundle
                IF(COUNT(pd.DiscountId) > 0, 1, 0) AS HasDiscount
            FROM
                {dbName}.Products p
                INNER JOIN {dbName}.ProductStocks ps ON p.Id = ps.ProductId
                INNER JOIN {dbName}.ProductCategories pc ON p.CategoryId = pc.Id
                LEFT JOIN {dbName}.ProductBundles pb ON p.Id = pb.ProductId
            GROUP BY
                p.Id";
            var results = await _db.QueryAsync<ProductDetailsResponse>(query);
            var productList = results.ToList();

            foreach (var product in productList)
            {
                if (product.IsBundle)
                {
                    const string bundlesQuery = @"
                    SELECT
                        pb.BundleId AS Id,
                        pb.BundledProductId AS ProductId,
                        pb.Quantity
                    FROM
                        {dbName}.ProductBundles pb
                    WHERE
                        pb.ProductId = @ProductId";
                    product.Bundles = await _db.QueryAsync<ProductBundles>(bundlesQuery, new { ProductId = product.Id });
                }
                if (product.HasDiscount)
                {
                    const string discountsQuery = @"
                    SELECT
                        pd.DiscountId AS Id,
                        pd.ProductId,
                        pd.DiscountValue,
                        pd.DiscountType
                    FROM
                        {dbName}.ProductDiscounts pd
                    WHERE
                        pd.ProductId = @ProductId";
                    product.Discounts = await _db.QueryAsync<ProductDiscounts>(discountsQuery, new { ProductId = product.Id });
                }
            }
            return productList;
        }
    }
}
