using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;

namespace Infrastructure.Repositories
{
    public class ProductsRepository : GenericRepository<Products, ProductsFilter>, IProductsRepository
    {
        protected readonly string _tableDiscountName;
        protected readonly string _tableBundleName;
        protected readonly string _tableCategoryName;
        public ProductsRepository(IDBFactory context) : base(context)
        {
            _tableDiscountName = typeof(ProductDiscounts).Name;
            _tableBundleName = typeof(ProductBundles).Name;
            _tableCategoryName = typeof(ProductCategories).Name;
        }

        public async Task AddProductBundles(ProductBundles bundles, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(bundles);
            var columnNames = string.Join(", ", propertyNames);
            var valuePlaceholders = string.Join(", ", propertyNames.Select(name => "@" + name));

            var query = $"INSERT INTO {_tableBundleName} ({columnNames}) VALUES ({valuePlaceholders})";

            await _db.ExecuteAsync(query, bundles);
        }

        public async Task AddProductDiscounts(ProductDiscounts discount, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(discount);
            var columnNames = string.Join(", ", propertyNames);
            var valuePlaceholders = string.Join(", ", propertyNames.Select(name => "@" + name));

            var query = $"INSERT INTO {_tableDiscountName} ({columnNames}) VALUES ({valuePlaceholders})";

            await _db.ExecuteAsync(query, discount);
        }

        public async Task DeleteProductBundles(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            await _db.ExecuteAsync($"DELETE FROM {_tableBundleName} WHERE Id = @Id", new { Id = id });
        }

        public async Task DeleteProductDiscounts(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            await _db.ExecuteAsync($"DELETE FROM {_tableDiscountName} WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateProductBundles(int id, ProductBundles bundles, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(bundles);
            var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

            var query = $"UPDATE {_tableBundleName} SET {setClause} WHERE Id = @Id";

            await _db.ExecuteAsync(query, bundles);
        }

        public async Task UpdateProductDiscounts(int id, ProductDiscounts discount, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(discount);
            var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

            var query = $"UPDATE {_tableDiscountName} SET {setClause} WHERE Id = @Id";

            await _db.ExecuteAsync(query, discount);
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

        public async Task AddProductCategories(ProductCategories categories, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(categories);
            var columnNames = string.Join(", ", propertyNames);
            var valuePlaceholders = string.Join(", ", propertyNames.Select(name => "@" + name));

            var query = $"INSERT INTO {_tableCategoryName} ({columnNames}) VALUES ({valuePlaceholders})";

            await _db.ExecuteAsync(query, categories);
        }

        public async Task UpdateProductCategories(int id, ProductCategories categories, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(categories);
            var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

            var query = $"UPDATE {_tableCategoryName} SET {setClause} WHERE Id = @Id";

            await _db.ExecuteAsync(query, categories);
        }

        public Task<IEnumerable<Products>> GetByFilter(string dbName, ProductsFilter filters)
        {
            throw new NotImplementedException();
        }

        public Task<int> CountWithFilter(string dbName, ProductsFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}
