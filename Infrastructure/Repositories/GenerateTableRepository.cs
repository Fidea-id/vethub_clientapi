using Dapper;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using Domain.Entities.Models.Clients.XPO;
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
            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = $"SELECT Version FROM SchemaVersion Where Note = '{init}'";

            var result = await _db.QueryFirstOrDefaultAsync<string>(query);
            bool dataExists = !string.IsNullOrEmpty(result); // Check if result is not null or empty
            return dataExists;
        }

        public async Task<bool> CheckSchemaVersion(string dbName, int version)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = $"SELECT Version FROM SchemaVersion Where Note = 'update_scheme' AND Version = '{version}'";

            var result = await _db.QueryFirstOrDefaultAsync<string>(query);
            bool dataExists = !string.IsNullOrEmpty(result); // Check if result is not null or empty
            return dataExists;
        }

        public async Task GenerateAllTable(string dbName)
        {
            AutoCreateDB(dbName);

            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
            var parameters = new { Version = 1, Note = "init_scheme" };
            await _db.ExecuteAsync(query, parameters);
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

        public async Task UpdateTable(string dbName, int newVersion)
        {
            AutoCreateDB(dbName);

            var _db = _dbFactory.GetDbConnection(dbName, true);
            var query = "INSERT INTO SchemaVersion (Version, Note) VALUES (@Version, @Note)";
            var parameters = new { Version = newVersion, Note = "update_scheme" };
            await _db.ExecuteAsync(query, parameters);
        }

        private void AutoCreateDB(string dbName)
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
                typeof(NotificationsXPO),
                typeof(ProductStockHistoricalXPO),
                typeof(OpnamePatientsXPO),
                typeof(OpnamesXPO),
                typeof(EventLogsXPO),
                typeof(ClinicConfigXPO),
                // Add more model types here
            };
            using (var updateDataLayer = XpoDefault.GetDataLayer(conn, dict, AutoCreateOption.DatabaseAndSchema))
            {
                updateDataLayer.UpdateSchema(false, dict.CollectClassInfos(entityTypes));
            }
        }
    }
}