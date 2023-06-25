using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace Infrastructure.Data
{
    public interface IDBFactory
    {
        IDbConnection GetDbConnection(string dbName, bool isNew = false);
        Dictionary<string, IDbConnection> GetConnectionCache();
    }

    public class DBFactory : IDBFactory
    {
        private readonly IConfiguration _configuration;
        private Dictionary<string, IDbConnection> _connectionCache;
        public DBFactory(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionCache = new Dictionary<string, IDbConnection>();
        }

        public Dictionary<string, IDbConnection> GetConnectionCache()
        {
            return _connectionCache;
        }
        public IDbConnection GetDbConnection(string dbName, bool isNew = false)
        {
            if (_connectionCache.TryGetValue(dbName, out var existingConnection))
            {
                // Connection exists in the cache, return the existing connection
                return existingConnection;
            }

            var environment = _configuration.GetSection("MyAppSettings")["Environment"];
            var connectionString = "";

            if (environment == "LOCAL")
            {
                // Local connection string
                connectionString = $"Server=localhost;port=3306;Database={dbName};userid=root;password=;sslmode=none;";
            }
            else
            {
                // Production connection string
                connectionString = $"Server=localhost;port=3306;Database={dbName};userid=adminmaster;password=SAl@k2lu87CO6oNOfApuB1nu21YOca;Allow User Variables=true";
            }

            var newConnection = new MySqlConnection(connectionString);

            //check is new
            if (isNew)
            {
                var tempConnectionString = "";
                if (environment == "LOCAL")
                {
                    // Local connection string
                    tempConnectionString = $"Server=localhost;port=3306;Database=VethubMaster;userid=root;password=;sslmode=none;";
                }
                else
                {
                    // Production connection string
                    tempConnectionString = $"Server=localhost;port=3306;Database=vethubmaster;userid=adminmaster;password=SAl@k2lu87CO6oNOfApuB1nu21YOca;Allow User Variables=true";
                }
                using (var tempConnection = new MySqlConnection(tempConnectionString))
                {
                    tempConnection.Open();

                    // Check if the database exists, if not, create it
                    if (!DatabaseExists(dbName, tempConnection))
                    {
                        CreateDatabase(dbName, tempConnection);
                    }

                    tempConnection.Close();
                }
            }

            // Add the new connection to the cache
            _connectionCache[dbName] = newConnection;

            return newConnection;
        }

        private bool DatabaseExists(string dbName, IDbConnection connection)
        {
            string query = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{dbName}';";
            var result = connection.ExecuteScalar<string>(query) != null;
            return result;
        }

        private void CreateDatabase(string dbName, IDbConnection connection)
        {
            string createDbQuery = $"CREATE DATABASE `{dbName}`;";
            connection.Execute(createDbQuery);
        }
    }
}

