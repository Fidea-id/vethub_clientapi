using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ProductBundlesRepository : GenericRepository<ProductBundles, ProductBundlesFilter>, IProductBundlesRepository
    {
        protected readonly string _tableBundleName;
        public ProductBundlesRepository(IDBFactory context) : base(context)
        {
            _tableBundleName = typeof(ProductBundles).Name;
        }

        public async Task<IEnumerable<ProductBundleDetailResponse>> GetProductBundlesByProduct(int productId, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            const string query = @"
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
            var result = await _db.QueryAsync<ProductBundleDetailResponse>(query, new { ProductId = productId });
            return result;
        }
    }
}
