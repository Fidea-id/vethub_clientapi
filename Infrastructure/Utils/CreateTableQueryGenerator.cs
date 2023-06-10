using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
            // Check if the model has a property with the [Key] attribute
            var keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            if (keyProperty != null)
            {
                var keyPropertyName = keyProperty.Name;
                var keySqlType = GetSqlType(keyProperty.PropertyType);

                queryBuilder.Append($"{keyPropertyName} {keySqlType} PRIMARY KEY AUTO_INCREMENT, ");
            }
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyName = property.Name;
                // Skip the property if it is the key property
                if (property == keyProperty)
                {
                    continue;
                }
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
            Type underlyingType = Nullable.GetUnderlyingType(propertyType);

            if (underlyingType != null)
            {
                // Nullable type, get the SQL type for the underlying type and append "DEFAULT NULL" to indicate nullability
                string sqlType = GetSqlType(underlyingType);
                return $"{sqlType} DEFAULT NULL";
            }
            else if (propertyType == typeof(int))
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
            else if (propertyType.IsEnum) // Handle enum type
            {
                var enumValues = string.Join("', '", Enum.GetNames(propertyType));
                return $"ENUM('{enumValues}')";
            }
            // Handle other types as needed

            return "UNKNOWN";
        }
        public static IEnumerable<(string Name, string Value)> GetPropertyNames<T>(T entity)
        {
            return entity.GetType().GetProperties().Select(property =>
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    var datetimeValue = (DateTime)property.GetValue(entity);
                    return (property.Name, datetimeValue.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    return (property.Name, property.GetValue(entity).ToString());
                }
            });
        }
        public static string GenerateInsertQuery(string tableName, object record)
        {
            var properties = record.GetType().GetProperties().Where(p => p.Name != "Id"); // Exclude the Id property
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"INSERT INTO {tableName} (");
            for (int i = 0; i < properties.Count(); i++)
            {
                var property = properties.ElementAt(i);
                var propertyName = property.Name;
                // Exclude properties that are not objects or collections
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !property.PropertyType.IsArray)
                {
                    var subProperties = property.PropertyType.GetProperties();

                    foreach (var subProperty in subProperties)
                    {
                        var subPropertyName = subProperty.Name;
                        queryBuilder.Append(subPropertyName);

                        // Add a comma separator for all lines except the last one
                        if (i < properties.Count() - 1 || subPropertyName != subProperties.Last().Name)
                        {
                            queryBuilder.Append(", ");
                        }
                    }
                }
                else
                {
                    queryBuilder.Append(propertyName);

                    // Add a comma separator for all lines except the last one
                    if (i < properties.Count() - 1)
                    {
                        queryBuilder.Append(", ");
                    }
                }
            }
            queryBuilder.Append(") VALUES (");
            for (int i = 0; i < properties.Count(); i++)
            {
                var property = properties.ElementAt(i);
                var propertyValue = property.GetValue(record);

                // Check if the property belongs to BaseEntity and set default values
                if (property.DeclaringType == typeof(BaseEntity))
                {
                    if (property.Name == "IsActive")
                    {
                        propertyValue = 1; // Set IsActive to true
                    }
                    else if (property.Name == "CreatedAt" || property.Name == "UpdatedAt")
                    {
                        propertyValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Format the datetime value
                    }
                }

                // Exclude properties that are not objects or collections
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !property.PropertyType.IsArray)
                {
                    var subProperties = property.PropertyType.GetProperties();

                    foreach (var subProperty in subProperties)
                    {
                        var subPropertyValue = subProperty.GetValue(propertyValue);

                        queryBuilder.Append($"'{subPropertyValue}'");

                        // Add a comma separator for all lines except the last one
                        if (i < properties.Count() - 1 || subProperty != subProperties.Last())
                        {
                            queryBuilder.Append(", ");
                        }
                    }
                }
                else
                {
                    queryBuilder.Append($"'{propertyValue}'");

                    // Add a comma separator for all lines except the last one
                    if (i < properties.Count() - 1)
                    {
                        queryBuilder.Append(", ");
                    }
                }
            }
            queryBuilder.Append(");");
            return queryBuilder.ToString();
        }
    }
}
