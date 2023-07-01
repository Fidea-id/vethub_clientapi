namespace Infrastructure.Utils
{
    public static class SchemaUpdater
    {
        public static string GetConnectionString(string environment, string dbName)
        {
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
            return connectionString;
        }
    }
}
