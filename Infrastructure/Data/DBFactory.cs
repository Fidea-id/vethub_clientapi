using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
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
                connectionString = $"Server=localhost;port=3306;Database={dbName};userid=savenwinadmin;password=3iMakiMudiWoT15573k2y1TEV7fal4!;Allow User Variables=true";
            }

            var newConnection = new MySqlConnection(connectionString);

            //check is new
            if (isNew)
            {
                // Check if the database exists, if not, create it
                if (!DatabaseExists(dbName, newConnection))
                {
                    CreateDatabase(dbName, newConnection);
                }
            }

            // Add the new connection to the cache
            _connectionCache[dbName] = newConnection;

            return newConnection;
        }

        private bool DatabaseExists(string dbName, IDbConnection connection)
        {
            string query = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{dbName}';";
            return connection.ExecuteScalar<string>(query) != null;
        }

        private void CreateDatabase(string dbName, IDbConnection connection)
        {
            string createDbQuery = $"CREATE DATABASE `{dbName}`;";
            connection.Execute(createDbQuery);
        }
    }
}

