using Dapper;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

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
            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = $"SELECT Version FROM SchemaVersion Where Note = '{init}'";

            var result = await _db.QueryFirstOrDefaultAsync<string>(query);
            bool dataExists = !string.IsNullOrEmpty(result); // Check if result is not null or empty
            return dataExists;
        }

        public async Task GenerateAllTable(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);

            var batchQueries = new List<string>();

            var models = new List<Type>
            {
                typeof(SchemaVersion),
                typeof(Profile),
                typeof(Services),
                typeof(Animals),
                typeof(Breeds),
                typeof(Owners),
                typeof(Patients),
                typeof(Products),
                typeof(ProductStocks),
                typeof(ProductBundles),
                typeof(ProductDiscounts),
                typeof(ProductCategories),
                typeof(Appointments),
                typeof(AppointmentsStatus),
                // Add more model types here
            };
            foreach (var modelType in models)
            {
                string createTableQuery = QueryGenerator.GenerateCreateTableQuery(modelType);
                batchQueries.Add(createTableQuery);
            }
            string batchQuery = string.Join(" ", batchQueries);
            await _db.ExecuteAsync(batchQuery);
        }

        public async Task GenerateTableField(string dbName, JObject fields)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var batchQueries = new List<string>();

            foreach (var entry in fields)
            {
                string tableName = entry.Key;
                var data = entry.Value;

                if (data is JArray dataList)
                {
                    // Generate INSERT statements for multiple records
                    foreach (var record in dataList)
                    {
                        // JsonConvert Deserialize to TableName Object class
                        string insertQuery = QueryGenerator.GenerateInsertQuery(tableName, record);
                        batchQueries.Add(insertQuery);
                    }
                }
                else
                {
                    // Generate INSERT statement for a single record
                    // JsonConvert Deserialize to TableName Object class
                    string insertQuery = QueryGenerator.GenerateInsertQuery(tableName, data);
                    batchQueries.Add(insertQuery);
                }
            }

            string batchQuery = string.Join(" ", batchQueries);
            await _db.ExecuteAsync(batchQuery);
        }

        public async Task SetInitSchema(string dbName, string init)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
            var parameters = new { Version = "1.0", Note = init };
            await _db.ExecuteAsync(query, parameters);
        }

        public async Task UpdateTable(string dbName, int expectedVersion)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);

            var currentVersion = await GetCurrentSchemaVersion(_db);

            if (currentVersion < expectedVersion)
            {
                var migrationScript = GenerateMigrationScript(currentVersion, expectedVersion);
                await _db.ExecuteAsync(migrationScript);

                // Update the schema version in the database
                await UpdateSchemaVersion(_db, expectedVersion);
            }
        }

        private string GenerateMigrationScript(int currentVersion, int expectedVersion)
        {
            var migrationScript = new StringBuilder();

            if (currentVersion < expectedVersion)
            {
                // Generate migration script based on the differences between the current and expected versions
                if (currentVersion == 0)
                {
                    // If the current version is 0, it means the schema is being created for the first time
                    migrationScript.AppendLine("CREATE TABLE IF NOT EXISTS MyTable (");
                    migrationScript.AppendLine("    Id INT PRIMARY KEY AUTO_INCREMENT,");
                    migrationScript.AppendLine("    Name VARCHAR(50) NOT NULL");
                    migrationScript.AppendLine(");");
                }

                // Example: Adding a new column 'Age' to the existing schema
                if (currentVersion < 2 && expectedVersion >= 2)
                {
                    migrationScript.AppendLine("ALTER TABLE MyTable ADD COLUMN Age INT;");
                }

                // Example: Modifying an existing column 'Name' in the schema
                if (currentVersion < 3 && expectedVersion >= 3)
                {
                    migrationScript.AppendLine("ALTER TABLE MyTable MODIFY COLUMN Name VARCHAR(100);");
                }

                // ... Add more migration steps as needed ...

                // Update the schema version to the expected version
                migrationScript.AppendLine($"UPDATE Version SET SchemaVersion = {expectedVersion};");
            }

            return migrationScript.ToString();
        }

        private async Task<int> GetCurrentSchemaVersion(IDbConnection dbConnection)
        {
            // Retrieve the current schema version from the table
            var query = $"SELECT Version FROM SchemaVersion";
            var currentVersion = await dbConnection.ExecuteScalarAsync<int>(query);
            return currentVersion;
        }

        private async Task UpdateSchemaVersion(IDbConnection dbConnection, int version)
        {
            // Update the existing schema version in the table
            var query = $"UPDATE SchemaVersion SET Version = @Version";
            var parameters = new { Version = version };
            await dbConnection.ExecuteAsync(query, parameters);
        }
    }
}