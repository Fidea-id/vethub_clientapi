using Dapper;
using Domain.Entities;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    internal class ProductStockHistoricalRepository : GenericRepository<ProductStockHistorical, BaseEntityFilter>, IProductStockHistoricalRepository
    {
        public ProductStockHistoricalRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductStockHistoricalResponse>> GetByProductHistoricalAsync(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = @"
                        SELECT 
                            psh.Stock,
                            psh.VolumeRemaining,
                            psh.CreatedAt AS Date,
                            psh.StockAfter,
                            psh.StockBefore,
                            psh.ProductId,
                            psh.ProfileId,
                            psh.Type,
                            p.Name AS ProductName,
                            COALESCE(pr.Name, '') AS ProfileName
                        FROM ProductStockHistorical psh
                        LEFT JOIN Products p ON psh.ProductId = p.Id
                        LEFT JOIN Profile pr ON psh.ProfileId = pr.Id;
                    ";
                var result = await _db.QueryAsync<ProductStockHistoricalResponse>(query);
                return result;
            }
        }
    }
}
