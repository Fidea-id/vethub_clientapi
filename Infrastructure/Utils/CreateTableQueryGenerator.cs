using System.Text;

namespace Infrastructure.Utils
{
    public static class CreateTableQueryGenerator
    {
        public static string GenerateCreateTableQuery(Type modelType)
        {
            var tableName = modelType.Name;
            var properties = modelType.GetProperties();

            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"CREATE TABLE IF NOT EXISTS {tableName} (");

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyName = property.Name;
                var sqlType = GetSqlType(property.PropertyType);

                queryBuilder.Append($"{propertyName} {sqlType}");

                // Add a comma separator for all lines except the last one
                if (i < properties.Length - 1)
                {
                    queryBuilder.Append(", ");
                }
            }

            queryBuilder.Append(");");
            return queryBuilder.ToString();
        }

        private static string GetSqlType(Type propertyType)
        {
            if (propertyType == typeof(int))
            {
                return "INT";
            }
            else if (propertyType == typeof(string))
            {
                return "VARCHAR(255)";
            }
            else if (propertyType == typeof(DateTime))
            {
                return "DATETIME";
            }
            else if (propertyType == typeof(bool))
            {
                return "TINYINT(1)";
            }
            else if (propertyType == typeof(double))
            {
                return "DOUBLE";
            }
            else if (propertyType == typeof(long))
            {
                return "BIGINT";
            }
            // Handle other types as needed

            return "UNKNOWN";
        }
    }
}
