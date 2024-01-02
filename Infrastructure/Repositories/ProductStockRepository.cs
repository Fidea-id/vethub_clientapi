using Dapper;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using static Dapper.SqlMapper;

namespace Infrastructure.Repositories
{
    public class ProductStockRepository : GenericRepository<ProductStocks, NameBaseEntityFilter>, IProductStockRepository
    {
        public ProductStockRepository(IDBFactory context) : base(context)
        {
        }

        public async Task UpdateMinStock(int productId, double quantity, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            // Check if the product exists
            var product = await _db.QueryFirstOrDefaultAsync<ProductStocks>($"SELECT * FROM ProductStocks WHERE ProductId = @productId", new { productId });
            if (product == null) throw new Exception("Product not found");
            if (product.Stock < quantity) throw new Exception("Insufficient stock");

            // Create the SQL query to update only stock and updatedAt
            var query = $"UPDATE {_tableName} SET Stock = Stock - @quantity, UpdatedAt = @updatedAt WHERE ProductId = @productId";

            // Create an anonymous object to pass the parameters
            var parameters = new
            {
                quantity,
                updatedAt = DateTime.Now, // You may need to adjust this based on your timezone and requirements
                productId
            };

            // Execute the update query
            await _db.ExecuteAsync(query, parameters);
        }

        public async Task UpdateAddStock(int productId, double quantity, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            // Create the SQL query to update only stock and updatedAt
            var query = $"UPDATE {_tableName} SET Stock = Stock + @quantity, UpdatedAt = @updatedAt WHERE ProductId = @productId";

            // Create an anonymous object to pass the parameters
            var parameters = new
            {
                quantity,
                updatedAt = DateTime.Now, // You may need to adjust this based on your timezone and requirements
                productId
            };

            // Execute the update query
            await _db.ExecuteAsync(query, parameters);
        }
    }
}
