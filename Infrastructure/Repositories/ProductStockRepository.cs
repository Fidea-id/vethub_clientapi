using Dapper;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using static Dapper.SqlMapper;
using System.Text;

namespace Infrastructure.Repositories
{
    public class ProductStockRepository : GenericRepository<ProductStocks, NameBaseEntityFilter>, IProductStockRepository
    {
        public ProductStockRepository(IDBFactory context) : base(context)
        {
        }

        public async Task UpdateMinStock(int productId, double quantity, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {

                // Check if the product exists
                var product = await _db.QueryFirstOrDefaultAsync<ProductStocks>($"SELECT * FROM ProductStocks WHERE ProductId = @productId AND IsActive = 1", new { productId });
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
        }

        public async Task UpdateAddStock(int productId, double quantity, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
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

        public async Task ApplyMedicalRecordStockChanges(string dbName, IEnumerable<MedicalRecordStockChangeRequest> changes)
        {
            var stockChanges = changes.Where(change => change.ProductId > 0 && change.Quantity != 0).ToList();
            if (stockChanges.Count == 0)
            {
                return;
            }

            using var db = _dbFactory.GetDbConnection(dbName);
            using var transaction = db.BeginTransaction();

            try
            {
                var productIds = stockChanges.Select(change => change.ProductId).Distinct().ToArray();
                var stockRows = await db.QueryAsync<ProductStocks>(
                    "SELECT * FROM ProductStocks WHERE ProductId IN @ProductIds ORDER BY Id FOR UPDATE",
                    new { ProductIds = productIds }, transaction);
                var stocksByProductId = stockRows
                    .GroupBy(stock => stock.ProductId)
                    .ToDictionary(group => group.Key, group => group.First());
                var historicalRows = new List<ProductStockHistorical>();
                var timestamp = DateTime.Now;

                foreach (var change in stockChanges)
                {
                    if (!stocksByProductId.TryGetValue(change.ProductId, out var stock))
                    {
                        throw new Exception($"Product stock not found for product {change.ProductId}");
                    }

                    // Legacy records can have zero volume. Treat one stock unit as one volume unit.
                    if (stock.Volume == 0)
                    {
                        stock.Volume = 1;
                    }

                    var stockBefore = stock.Stock;
                    var totalVolume = (stock.Stock * stock.Volume) + stock.VolumeRemaining;
                    var updatedVolume = change.RestoreStock
                        ? totalVolume + change.Quantity
                        : Math.Max(0, totalVolume - change.Quantity);

                    stock.Stock = Math.Floor(updatedVolume / stock.Volume);
                    stock.VolumeRemaining = updatedVolume % stock.Volume;
                    stock.UpdatedAt = timestamp;

                    historicalRows.Add(new ProductStockHistorical
                    {
                        ProfileId = change.ProfileId,
                        ProductId = stock.ProductId,
                        Stock = Math.Floor(change.Quantity / stock.Volume),
                        StockBefore = stockBefore,
                        StockAfter = stock.Stock,
                        VolumeRemaining = stock.VolumeRemaining,
                        Type = change.Type,
                        IsActive = true,
                        CreatedAt = timestamp,
                        UpdatedAt = timestamp
                    });
                }

                var stockParameters = new DynamicParameters();
                var stockUpdateSql = new StringBuilder();
                var stockIndex = 0;
                foreach (var stock in stocksByProductId.Values)
                {
                    stockUpdateSql.Append($"UPDATE ProductStocks SET Stock = @Stock{stockIndex}, Volume = @Volume{stockIndex}, VolumeRemaining = @VolumeRemaining{stockIndex}, UpdatedAt = @UpdatedAt{stockIndex} WHERE Id = @Id{stockIndex};");
                    stockParameters.Add($"Stock{stockIndex}", stock.Stock);
                    stockParameters.Add($"Volume{stockIndex}", stock.Volume);
                    stockParameters.Add($"VolumeRemaining{stockIndex}", stock.VolumeRemaining);
                    stockParameters.Add($"UpdatedAt{stockIndex}", stock.UpdatedAt);
                    stockParameters.Add($"Id{stockIndex}", stock.Id);
                    stockIndex++;
                }
                await db.ExecuteAsync(stockUpdateSql.ToString(), stockParameters, transaction);

                var historicalParameters = new DynamicParameters();
                var historicalInsertSql = new StringBuilder("INSERT INTO ProductStockHistorical (ProfileId, ProductId, Stock, StockBefore, StockAfter, VolumeRemaining, Type, IsActive, CreatedAt, UpdatedAt) VALUES ");
                for (var index = 0; index < historicalRows.Count; index++)
                {
                    var historical = historicalRows[index];
                    if (index > 0)
                    {
                        historicalInsertSql.Append(", ");
                    }

                    historicalInsertSql.Append($"(@ProfileId{index}, @ProductId{index}, @Stock{index}, @StockBefore{index}, @StockAfter{index}, @VolumeRemaining{index}, @Type{index}, @IsActive{index}, @CreatedAt{index}, @UpdatedAt{index})");
                    historicalParameters.Add($"ProfileId{index}", historical.ProfileId);
                    historicalParameters.Add($"ProductId{index}", historical.ProductId);
                    historicalParameters.Add($"Stock{index}", historical.Stock);
                    historicalParameters.Add($"StockBefore{index}", historical.StockBefore);
                    historicalParameters.Add($"StockAfter{index}", historical.StockAfter);
                    historicalParameters.Add($"VolumeRemaining{index}", historical.VolumeRemaining);
                    historicalParameters.Add($"Type{index}", historical.Type);
                    historicalParameters.Add($"IsActive{index}", historical.IsActive);
                    historicalParameters.Add($"CreatedAt{index}", historical.CreatedAt);
                    historicalParameters.Add($"UpdatedAt{index}", historical.UpdatedAt);
                }
                await db.ExecuteAsync(historicalInsertSql.ToString(), historicalParameters, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
