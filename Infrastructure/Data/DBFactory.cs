using Infrastructure.Utils;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Data;

namespace Infrastructure.Data
{
    public interface IDBFactory
    {
        IDbConnection GetDbConnection(string dbName);
        IDbConnection GetMasterDbConnection();
        string GetConnectionString(string dbName);
        //Dictionary<string, IDbConnection> GetConnectionCache();
    }

    public class DBFactory : IDBFactory
    {
        private readonly IConfiguration _configuration;
        public DBFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection GetDbConnection(string dbName)
        {
            var environment = _configuration.GetSection("MyAppSettings")["Environment"];
            var connectionString = SchemaUpdater.GetConnectionString(environment, dbName)
                       + ";Pooling=true;Min Pool Size=0;Max Pool Size=25;Connection Timeout=60;";

            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public IDbConnection GetMasterDbConnection()
        {
            var environment = _configuration.GetSection("MyAppSettings")["Environment"];
            var connectionString = SchemaUpdater.GetMasterConnectionString(environment)
                       + ";Pooling=true;Min Pool Size=0;Max Pool Size=10;Connection Timeout=60;";

            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }


        //private bool DatabaseExists(string dbName, IDbConnection connection)
        //{
        //    string query = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{dbName}';";
        //    var result = connection.ExecuteScalar<string>(query) != null;
        //    return result;
        //}

        //private void CreateDatabase(string dbName, IDbConnection connection)
        //{
        //    string createDbQuery = $"CREATE DATABASE `{dbName}`;";
        //    connection.Execute(createDbQuery);
        //}

        public string GetConnectionString(string dbName)
        {
            var environment = _configuration.GetSection("MyAppSettings")["Environment"];
            return SchemaUpdater.GetConnectionString(environment, dbName)
                       + ";Pooling=true;Min Pool Size=0;Max Pool Size=50;Connection Timeout=30;";
        }
    }
}

