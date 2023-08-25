using Dapper;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Domain.Entities.Models.Clients;
using Domain.Entities.Models.Clients.XPO;
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
            // Construct the connection string
            string conn = _dbFactory.GetConnectionString(dbName);

            XPDictionary dict = new ReflectionDictionary();

            var entityTypes = new List<Type>
            {
                typeof(SchemaVersionXPO),
                typeof(ProfileXPO),
                typeof(ServicesXPO),
                typeof(AnimalsXPO),
                typeof(BreedsXPO),
                typeof(OwnersXPO),
                typeof(PatientsXPO),
                typeof(ProductsXPO),
                typeof(ProductStocksXPO),
                typeof(ProductBundlesXPO),
                typeof(ProductDiscountsXPO),
                typeof(ProductCategoriesXPO),
                typeof(AppointmentsXPO),
                typeof(AppointmentsActivityXPO),
                typeof(AppointmentsStatusXPO),
                typeof(PatientsStatisticXPO),
                typeof(DiagnosesXPO),
                typeof(OrdersXPO),
                typeof(OrdersDetailXPO),
                typeof(OrdersPaymentXPO),
                typeof(ClinicsXPO),
                typeof(PaymentMethodXPO),
                typeof(MedicalRecordsXPO),
                typeof(MedicalRecordsDiagnosesXPO),
                typeof(MedicalRecordsNotesXPO),
                typeof(MedicalRecordsPrescriptionsXPO),
                typeof(PrescriptionFrequentsXPO),
                // Add more model types here
            };
            using (var updateDataLayer = XpoDefault.GetDataLayer(conn, dict, AutoCreateOption.DatabaseAndSchema))
            {
                updateDataLayer.UpdateSchema(false, dict.CollectClassInfos(entityTypes));
            }

            //var _db = _dbFactory.GetDbConnection(dbName, true);

            //var batchQueries = "CREATE TABLE IF NOT EXISTS SchemaVersion (Version INT, Note VARCHAR(255)); CREATE TABLE IF NOT EXISTS Profile (Id INT PRIMARY KEY AUTO_INCREMENT, GlobalId INT, Name VARCHAR(255), Entity VARCHAR(255), Email VARCHAR(255), Photo VARCHAR(255), Roles VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Services (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), Duration INT, DurationType VARCHAR(255), IsActive TINYINT(1), Price DOUBLE, CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Animals (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Breeds (Id INT PRIMARY KEY AUTO_INCREMENT, AnimalsId INT, Name VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Owners (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), Photo VARCHAR(255), Email VARCHAR(255), Title VARCHAR(255), Address VARCHAR(255), PhoneNumber VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Patients (Id INT PRIMARY KEY AUTO_INCREMENT, OwnersId INT, Name VARCHAR(255), Photo VARCHAR(255), Species VARCHAR(255), Breed VARCHAR(255), Gender VARCHAR(255), DateOfBirth DATETIME, Vaccinated TINYINT(1), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Products (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), Description VARCHAR(255), CategoryId INT, Price DOUBLE, IsBundle TINYINT(1), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS ProductStocks (Id INT PRIMARY KEY AUTO_INCREMENT, ProductId INT, Stock DOUBLE, Volume VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS ProductBundles (Id INT PRIMARY KEY AUTO_INCREMENT, BundleId INT, ItemId INT, Quantity DOUBLE, IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS ProductDiscounts (Id INT PRIMARY KEY AUTO_INCREMENT, ProductId INT, Description VARCHAR(255), DiscountValue DOUBLE, DiscountType ENUM('Percentage', 'Amount'), StartDate DATETIME DEFAULT NULL, EndDate DATETIME DEFAULT NULL, IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS ProductCategories (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS Appointments (Id INT PRIMARY KEY AUTO_INCREMENT, OwnersId INT, PatientsId INT, Date DATETIME, StaffId INT, ServiceId INT, StatusId INT, Notes VARCHAR(255), IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME); CREATE TABLE IF NOT EXISTS AppointmentsStatus (Id INT PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(255), Description VARCHAR(255), Weight INT, IsActive TINYINT(1), CreatedAt DATETIME, UpdatedAt DATETIME);";
            //await _db.ExecuteAsync(batchQueries);
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