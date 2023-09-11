using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

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
                COALESCE(Sum(ps.Stock), 0) AS Stock, 
                ps.Volume,
                p.CategoryId,
                pc.Name AS Category,
                p.Price,
	             p.IsBundle,
                p.IsActive,
	             IF(COUNT(CASE WHEN pd.Id IS NOT NULL AND pd.IsActive = true AND NOW() BETWEEN pd.StartDate AND pd.EndDate THEN pd.Id END) > 0, 1, 0) AS HasDiscount
            FROM
                Products p
                JOIN ProductCategories pc ON p.CategoryId = pc.Id
                LEFT JOIN ProductStocks ps ON p.Id = ps.ProductId
                LEFT JOIN ProductBundles pb ON p.Id = pb.BundleId
                LEFT JOIN ProductDiscounts pd ON p.Id = pd.ProductId
            WHERE
	            p.IsActive = TRUE And
                p.Id = @ProductId
            GROUP BY
                p.Id,
                ps.Volume";
            var result = await _db.QueryFirstOrDefaultAsync<ProductDetailsResponse>(query, new { ProductId = id });
            if (result == null)
                return null;

            if (result.IsBundle)
            {
                const string bundlesQuery = @"
                SELECT
                    pb.Id AS BundleId,
                    p_bundle.Name AS BundleName,
                    p_bundle.Price AS BundlePrice,
                    ps_bundle.Stock AS BundleStock,
                    pb.Quantity AS ItemQuantity,
                    p_item.Id AS ItemId,
                    p_item.Name AS ItemName,
                    p_item.Price AS ItemPrice,
                    ps_item.Stock AS ItemStock
                FROM
                    ProductBundles pb
                JOIN
                    Products p_bundle ON pb.BundleId = p_bundle.Id
                JOIN
                    Products p_item ON pb.ItemId = p_item.Id
                LEFT JOIN
                    ProductStocks ps_bundle ON pb.BundleId = ps_bundle.ProductId
                LEFT JOIN
                    ProductStocks ps_item ON pb.ItemId = ps_item.ProductId
                WHERE
                    pb.BundleId = @ProductId";
                result.BundlesItems = await _db.QueryAsync<ProductBundleDetailResponse>(bundlesQuery, new { ProductId = id });
            }

            if (result.HasDiscount)
            {
                const string discountsQuery = @"
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
                    Products p ON pd.ProductId = p.Id
                WHERE
                    pd.ProductId = @ProductId";
                result.Discounts = await _db.QueryAsync<ProductDiscountDetailResponse>(discountsQuery, new { ProductId = id });
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
                p.CategoryId,
                pc.Name AS Category,
                p.Price,
                p.IsBundle,
                COALESCE(Sum(ps.Stock), 0) AS Stock, 
                ps.Volume,
                p.IsActive,
                IF(COUNT(CASE WHEN pd.Id IS NOT NULL AND pd.IsActive = true AND NOW() BETWEEN pd.StartDate AND pd.EndDate THEN pd.Id END) > 0, 1, 0) AS HasDiscount
            FROM
                Products p
                JOIN ProductCategories pc ON p.CategoryId = pc.Id
                LEFT JOIN ProductStocks ps ON p.Id = ps.ProductId
                LEFT JOIN ProductBundles pb ON p.Id = pb.BundleId
                LEFT JOIN ProductDiscounts pd ON p.Id = pd.ProductId
            WHERE
               p.IsActive = TRUE
            GROUP BY
                p.Id,
                ps.Volume;";
            var results = await _db.QueryAsync<ProductDetailsResponse>(query);
            var productList = results.ToList();

            foreach (var product in productList)
            {
                if (product.IsBundle)
                {
                    const string bundlesQuery = @"
                    SELECT
                        pb.Id AS BundleId,
                        p_bundle.Name AS BundleName,
                        p_bundle.Price AS BundlePrice,
                        ps_bundle.Stock AS BundleStock,
                        pb.Quantity AS ItemQuantity,
                        p_item.Id AS ItemId,
                        p_item.Name AS ItemName,
                        p_item.Price AS ItemPrice,
                        ps_item.Stock AS ItemStock
                    FROM
                        ProductBundles pb
                    JOIN
                        Products p_bundle ON pb.BundleId = p_bundle.Id
                    JOIN
                        Products p_item ON pb.ItemId = p_item.Id
                    LEFT JOIN
                        ProductStocks ps_bundle ON pb.BundleId = ps_bundle.ProductId
                    LEFT JOIN
                        ProductStocks ps_item ON pb.ItemId = ps_item.ProductId
                    WHERE
                        pb.BundleId = @ProductId";
                    product.BundlesItems = await _db.QueryAsync<ProductBundleDetailResponse>(bundlesQuery, new { ProductId = product.Id });
                }
                if (product.HasDiscount)
                {
                    const string discountsQuery = @"
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
                      Products p ON pd.ProductId = p.Id
                    WHERE
                      pd.ProductId = @ProductId";
                    product.Discounts = await _db.QueryAsync<ProductDiscountDetailResponse>(discountsQuery, new { ProductId = product.Id });
                }
            }
            return productList;
        }
    }
}
