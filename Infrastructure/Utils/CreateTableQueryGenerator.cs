using System.Text;

namespace Infrastructure.Utils
{
    public static class CreateTableQueryGenerator
    {
        public static string GenerateCreateTableQuery(Type modelType)
        {
            var tableName = modelType.Name;
            var properties = modelType.GetProperties();

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {tableName} (");

            foreach (var property in properties)
            {
                var columnName = property.Name;
                var columnType = GetSqlType(property.PropertyType);
                sb.AppendLine($"{columnName} {columnType},");
            }

            sb.AppendLine(");");

            return sb.ToString();
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
