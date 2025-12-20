using Dapper;
using Domain.Entities;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System.Data;

namespace Infrastructure.Repositories
{
    public class MixedMedicineRepository : GenericRepository<MixedMedicine, BaseEntityFilter>, IMixedMedicineRepository
    {
        public MixedMedicineRepository(IDBFactory context) : base(context)
        {
        }
        public async Task<IEnumerable<MixedMedicineDetailResponse>> GetDetails(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                const string query = @"
                SELECT 
                    mm.*, 
                    mc.Id AS CompositionId, mc.MixedMedicineId, mc.ProductId, mc.QuantityPerUnit,
                    p.Name AS ProductName, p.Price AS ProductPrice, p.BoughtPrice AS ProductBoughtPrice
                FROM MixedMedicine mm
                LEFT JOIN MixedMedicineComposition mc ON mm.Id = mc.MixedMedicineId
                LEFT JOIN Products p ON p.Id = mc.ProductId
                WHERE mm.IsActive = true";

                var mixedMedicineDict = new Dictionary<int, MixedMedicineDetailResponse>();

                var result = await _db.QueryAsync<MixedMedicineDetailResponse, MixedMedicineCompositionResponse, MixedMedicineDetailResponse>(
                    query,
                    (mm, comp) =>
                    {
                        if (!mixedMedicineDict.TryGetValue(mm.Id, out var mixedMed))
                        {
                            mixedMed = mm;
                            mixedMed.Compositions = new List<MixedMedicineCompositionResponse>();
                            mixedMedicineDict[mixedMed.Id] = mixedMed;
                        }

                        if (comp != null && comp.ProductId != 0)
                            mixedMed.Compositions.Add(comp);

                        return mixedMed;
                    },
                    splitOn: "CompositionId"
                );

                await SetCurrentStocks(_db, mixedMedicineDict.Values);

                return mixedMedicineDict.Values;
            }
        }

        public async Task<MixedMedicineDetailResponse> GetDetailById(string dbName, int id)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                const string query = @"
                SELECT 
                    mm.*, 
                    mc.Id AS CompositionId, mc.MixedMedicineId, mc.ProductId, mc.QuantityPerUnit,
                    p.Name AS ProductName, p.Price AS ProductPrice, p.BoughtPrice AS ProductBoughtPrice
                FROM MixedMedicine mm
                LEFT JOIN MixedMedicineComposition mc ON mm.Id = mc.MixedMedicineId
                LEFT JOIN Products p ON p.Id = mc.ProductId
                WHERE mm.Id = @Id AND mm.IsActive = true";

                var mixedMedicineDict = new Dictionary<int, MixedMedicineDetailResponse>();

                var result = await _db.QueryAsync<MixedMedicineDetailResponse, MixedMedicineCompositionResponse, MixedMedicineDetailResponse>(
                    query,
                    (mm, comp) =>
                    {
                        if (!mixedMedicineDict.TryGetValue(mm.Id, out var mixedMed))
                        {
                            mixedMed = mm;
                            mixedMed.Compositions = new List<MixedMedicineCompositionResponse>();
                            mixedMedicineDict[mixedMed.Id] = mixedMed;
                        }

                        if (comp != null && comp.ProductId != 0)
                            mixedMed.Compositions.Add(comp);

                        return mixedMed;
                    },
                    new { Id = id },
                    splitOn: "CompositionId"
                );

                var final = mixedMedicineDict.Values.FirstOrDefault();

                if (final != null)
                    await SetCurrentStocks(_db, new List<MixedMedicineDetailResponse> { final });

                return final;
            }
        }

        public async Task<MixedMedicineDetailResponse> CreateMixedMedicine(string dbName, MixedMedicineDetailRequest data)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                const string insertMedicineQuery = @"
            INSERT INTO MixedMedicine (Name, Description, Price, Unit, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Price, @Unit, true, NOW(), NOW());
            SELECT LAST_INSERT_ID();";

                var mixedMedicineId = await _db.ExecuteScalarAsync<int>(insertMedicineQuery, data);

                const string insertCompositionQuery = @"
            INSERT INTO MixedMedicineComposition (MixedMedicineId, ProductId, QuantityPerUnit, IsActive, CreatedAt, UpdatedAt)
            VALUES (@MixedMedicineId, @ProductId, @QuantityPerUnit, true, NOW(), NOW());";

                foreach (var comp in data.Compositions)
                {
                    await _db.ExecuteAsync(insertCompositionQuery, new
                    {
                        MixedMedicineId = mixedMedicineId,
                        comp.ProductId,
                        comp.QuantityPerUnit
                    });
                }

                return await GetDetailById(dbName, mixedMedicineId);
            }
        }

        public async Task<MixedMedicineDetailResponse> UpdateMixedMedicine(string dbName, int id, MixedMedicineDetailRequest data)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                const string updateMedicineQuery = @"
            UPDATE MixedMedicine
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                Unit = @Unit,
                UpdatedAt = NOW()
            WHERE Id = @Id";

                await _db.ExecuteAsync(updateMedicineQuery, new { data.Name, data.Description, data.Price, data.Unit, Id = id });

                const string deleteOldCompositionQuery = @"
            DELETE FROM MixedMedicineComposition WHERE MixedMedicineId = @Id";

                await _db.ExecuteAsync(deleteOldCompositionQuery, new { Id = id });

                const string insertCompositionQuery = @"
            INSERT INTO MixedMedicineComposition (MixedMedicineId, ProductId, QuantityPerUnit, IsActive, CreatedAt, UpdatedAt)
            VALUES (@MixedMedicineId, @ProductId, @QuantityPerUnit, true, NOW(), NOW());";

                foreach (var comp in data.Compositions)
                {
                    await _db.ExecuteAsync(insertCompositionQuery, new
                    {
                        MixedMedicineId = id,
                        comp.ProductId,
                        comp.QuantityPerUnit
                    });
                }

                return await GetDetailById(dbName, id);

            }
        }

        private async Task SetCurrentStocks(IDbConnection _db, IEnumerable<MixedMedicineDetailResponse> mixedList)
        {
            foreach (var mixed in mixedList)
            {
                if (mixed.Compositions == null || !mixed.Compositions.Any())
                {
                    mixed.CurrentStock = 0;
                    continue;
                }

                var productIds = mixed.Compositions.Select(c => c.ProductId);
                const string stockQuery = @"
                SELECT ProductId, Stock
                FROM ProductStocks
                WHERE IsActive = true AND ProductId IN @ProductIds";

                var stocks = (await _db.QueryAsync<MixedMedicineStockDto>(stockQuery, new { ProductIds = productIds })).ToList();

                var minStock = double.MaxValue;

                foreach (var comp in mixed.Compositions)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == comp.ProductId)?.Stock ?? 0;
                    if (comp.QuantityPerUnit <= 0)
                        continue;

                    var available = Math.Floor(stock / comp.QuantityPerUnit);
                    minStock = Math.Min(minStock, available);
                }

                mixed.CurrentStock = minStock == double.MaxValue ? 0 : minStock;
            }
        }

        public async Task DeleteMixedMedicine(string dbName, int id)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                const string deactivateCompositionQuery = @"
                UPDATE MixedMedicineComposition 
                SET IsActive = false, UpdatedAt = NOW()
                WHERE MixedMedicineId = @Id";

                await _db.ExecuteAsync(deactivateCompositionQuery, new { Id = id });

                const string deactivateMedicineQuery = @"
                UPDATE MixedMedicine 
                SET IsActive = false, UpdatedAt = NOW()
                WHERE Id = @Id";

                await _db.ExecuteAsync(deactivateMedicineQuery, new { Id = id });
            }
        }
    }
}
