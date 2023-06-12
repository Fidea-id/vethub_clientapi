using Dapper;
using Domain.Entities.Filters;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Utils;

namespace Infrastructure.Repositories
{
    public class GenerateTableRepository : IGenerateTableRepository
    {
        private readonly IDBFactory _dbFactory;
        public GenerateTableRepository(IDBFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task GenerateAllTable(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);

            var batchQueries = new List<string>();

            var models = new List<Type>
            {
                typeof(Profile),
                typeof(Services),
                typeof(Owners),
                typeof(Patients),
                typeof(Products),
                typeof(ProductStocks),
                typeof(ProductBundles),
                typeof(ProductDiscounts),
                typeof(ProductCategories),
                // Add more model types here
            };
            foreach (var modelType in models)
            {
                string createTableQuery = CreateTableQueryGenerator.GenerateCreateTableQuery(modelType);
                batchQueries.Add(createTableQuery);
            }
            string batchQuery = string.Join(" ", batchQueries);
            await _db.ExecuteAsync(batchQuery);
        }
        public async Task GenerateTableField(string dbName, Dictionary<string, object> fields)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var batchQueries = new List<string>();

            foreach (var entry in fields)
            {
                string tableName = entry.Key;
                var data = entry.Value;

                if (data is IEnumerable<object> dataList)
                {
                    // Generate INSERT statements for multiple records
                    foreach (var record in dataList)
                    {
                        // JsonConvert Deserialize to TableName Object class
                        string insertQuery = CreateTableQueryGenerator.GenerateInsertQuery(tableName, record);
                        batchQueries.Add(insertQuery);
                    }
                }
                else
                {
                    // Generate INSERT statement for a single record
                    // JsonConvert Deserialize to TableName Object class
                    string insertQuery = CreateTableQueryGenerator.GenerateInsertQuery(tableName, data);
                    batchQueries.Add(insertQuery);
                }
            }

            string batchQuery = string.Join(" ", batchQueries);
            await _db.ExecuteAsync(batchQuery);
        }
    }
}