using Dapper;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Repositories
{
    public class GenerateTableRepository : IGenerateTableRepository
    {
        private readonly IDBFactory _dbFactory;
        public GenerateTableRepository(IDBFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<bool> CheckInitSchema(string dbName, string init)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = $"SELECT Version FROM SchemaVersion Where Note = '{init}'";

                var result = await _db.QueryFirstOrDefaultAsync<string>(query);
                bool dataExists = !string.IsNullOrEmpty(result); // Check if result is not null or empty
                return dataExists;
            }
        }

        public async Task<bool> CheckSchemaVersion(string dbName, int version)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = $"SELECT Version FROM SchemaVersion Where Note = 'update_scheme' AND Version = '{version}'";

                var result = await _db.QueryFirstOrDefaultAsync<string>(query);
                bool dataExists = !string.IsNullOrEmpty(result); // Check if result is not null or empty
                return dataExists;
            }
        }
        //public async Task GenerateAllTable(string dbName)
        //{
        //    AutoCreateDB(dbName);

        //    var _db = _dbFactory.GetDbConnection(dbName, true);
        //    var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
        //    var parameters = new { Version = 1, Note = "init_scheme" };
        //    await _db.ExecuteAsync(query, parameters);
        //}

        public async Task GenerateTableField(string dbName, JObject fields)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var batchQueries = new List<string>();

                foreach (var entry in fields)
                {
                    string tableName = entry.Key;
                    var data = entry.Value;

                    if (data is JArray arr)
                    {
                        foreach (var record in arr)
                            batchQueries.Add(QueryGenerator.GenerateInsertQuery(tableName, record));
                    }
                    else
                    {
                        batchQueries.Add(QueryGenerator.GenerateInsertQuery(tableName, data));
                    }
                }

                string batchQuery = string.Join(" ", batchQueries);
                await _db.ExecuteAsync(batchQuery);
            }
        }
        public async Task InsertUpdateSchemaVersion(string dbName, int version)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
                await _db.ExecuteAsync(query, new { Version = version, Note = "update_scheme" });
            }
        }

        public async Task SetInitSchema(string dbName, string init)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
                var parameters = new { Version = "1.0", Note = init };
                await _db.ExecuteAsync(query, parameters);
            }
        }

        //public async Task UpdateTable(string dbName, int newVersion)
        //{
        //    AutoCreateDB(dbName);

        //    var _db = _dbFactory.GetDbConnection(dbName);
        //    var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
        //    var parameters = new { Version = newVersion, Note = "update_scheme" };
        //    await _db.ExecuteAsync(query, parameters);
        //}
    }
}