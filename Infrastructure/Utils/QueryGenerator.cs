using Dapper;
using Domain.Entities;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;

namespace Infrastructure.Utils
{
    public static class QueryGenerator
    {
        public static string GenerateSelectOrCountQuery(string selectClause, bool isCount)
        {
            return isCount ? $"SELECT COUNT(*) {selectClause.Substring(selectClause.IndexOf("FROM"))}" : selectClause;
        }
        public static Tuple<string, DynamicParameters> GenerateFilterQuery<TFilter>(TFilter filters, string mainTableName, string joinQuery = null, List<string> selectColumns = null) where TFilter : BaseEntityFilter
        {
            // Build the WHERE clause based on the filters
            var whereClause = "";
            var parameters = new DynamicParameters();

            // Get the properties of the filter object
            var filterProperties = typeof(TFilter).GetProperties();

            // Handle searching
            if (!string.IsNullOrEmpty(filters.Search))
            {
                var searchValue = $"%{filters.Search}%";
                var stringProperties = typeof(TFilter).GetProperties()
                             .Where(p => p.PropertyType == typeof(string) && p.DeclaringType == typeof(TFilter));

                whereClause += "(";

                foreach (var property in stringProperties)
                {
                    whereClause += $"{mainTableName}.{property.Name} LIKE @SearchValue OR ";
                    parameters.Add("@SearchValue", searchValue);
                }

                whereClause = whereClause.TrimEnd("OR ".ToCharArray()) + ")";
            }

            foreach (var property in filterProperties)
            {
                var value = property.GetValue(filters);
                if (value != null && property.Name != "SortProp" && property.Name != "SortMode" && property.Name != "Skip" && property.Name != "Take" && property.Name != "Search")
                {
                    var paramName = $"@{property.Name}";
                    var propertyType = property.PropertyType;

                    if (propertyType == typeof(int) || propertyType == typeof(bool))
                    {
                        whereClause += $"{mainTableName}.{property.Name} = {paramName} AND ";
                        parameters.Add(paramName, value);
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        whereClause += $"{mainTableName}.{property.Name} = {paramName} AND ";
                        parameters.Add(paramName, value, DbType.DateTime);
                    }
                    else
                    {
                        whereClause += $"{mainTableName}.{property.Name} LIKE {paramName} AND ";
                        parameters.Add(paramName, $"%{value}%"); // Using wildcard '%' for exact matching
                    }
                }
            }

            // Remove the trailing "AND " from the where clause
            if (!string.IsNullOrEmpty(whereClause))
            {
                whereClause = "WHERE " + whereClause.TrimEnd("AND ".ToCharArray());
            }

            // Handle sorting
            var sortClause = "";
            if (!string.IsNullOrEmpty(filters.SortProp))
            {
                var sortMode = string.IsNullOrEmpty(filters.SortMode) ? "ASC" : filters.SortMode.ToUpper();
                sortClause = $"ORDER BY {mainTableName}.{filters.SortProp} {sortMode}";
            }

            //var limitClause = "";
            //if (filters.Skip.HasValue && filters.Take.HasValue)
            //{
            //    limitClause = $"LIMIT {filters.Skip.Value}, {filters.Take.Value}";
            //}
            //else if (filters.Take.HasValue)
            //{
            //    limitClause = $"LIMIT {filters.Take.Value}";
            //}

            // Construct the SELECT clause
            var selectClause = "SELECT ";
            if (selectColumns != null && selectColumns.Any())
            {
                selectClause += string.Join(", ", selectColumns);
            }
            else
            {
                selectClause += "*";
            }

            var query = $"{selectClause} FROM {mainTableName} {joinQuery} {whereClause} {sortClause}";

            return new Tuple<string, DynamicParameters>(query, parameters);
        }

        public static string GenerateFilteredLimitQuery(string selectOrCountQuery, int? skip, int? take)
        {
            var limitClause = "";

            if (take.HasValue)
            {
                if (skip.HasValue)
                {
                    limitClause = $"LIMIT {skip.Value}, {take.Value}";
                }
                else
                {
                    limitClause = $"LIMIT {take.Value}";
                }
            }

            return $"{selectOrCountQuery} {limitClause}";
        }

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
                // Set Max Length
                var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLengthAttribute != null)
                {
                    sqlType = $"{sqlType}({maxLengthAttribute.Length})";
                }

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
                var propertyValue = property.GetValue(entity);
                if (propertyValue == null)
                {
                    return (property.Name, null);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    var datetimeValue = (DateTime)propertyValue;
                    return (property.Name, datetimeValue.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else if (property.PropertyType.IsEnum) // Check if the property type is an enum
                {
                    var enumValue = (int)propertyValue; // Cast the enum to int
                    return (property.Name, enumValue.ToString());
                }
                else
                {
                    return (property.Name, propertyValue.ToString());
                }
            });
        }
        public static string GenerateInsertQuery(string tableName, JToken record)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append($"INSERT INTO {tableName} (");

            if (record is JObject obj)
            {
                var properties = obj.Properties().Where(p => p.Name != "Id"); // Exclude the Id property

                for (int i = 0; i < properties.Count(); i++)
                {
                    var property = properties.ElementAt(i);

                    queryBuilder.Append(property.Name);

                    // Add a comma separator for all lines except the last one
                    if (i < properties.Count() - 1)
                    {
                        queryBuilder.Append(", ");
                    }
                }

                queryBuilder.Append(") VALUES (");

                for (int i = 0; i < properties.Count(); i++)
                {
                    var property = properties.ElementAt(i);
                    var propertyValue = property.Value;

                    queryBuilder.Append($"'{propertyValue}'");

                    // Add a comma separator for all lines except the last one
                    if (i < properties.Count() - 1)
                    {
                        queryBuilder.Append(", ");
                    }
                }
            }
            else if (record is JArray array)
            {
                var records = array.Children<JObject>();

                if (records.Any())
                {
                    var properties = records.First().Properties().Where(p => p.Name != "Id"); // Exclude the Id property

                    foreach (var objRecord in records)
                    {
                        queryBuilder.Append($"(");

                        for (int i = 0; i < properties.Count(); i++)
                        {
                            var property = properties.ElementAt(i);
                            var propertyValue = objRecord[property.Name];

                            queryBuilder.Append($"'{propertyValue}'");

                            // Add a comma separator for all lines except the last one
                            if (i < properties.Count() - 1)
                            {
                                queryBuilder.Append(", ");
                            }
                        }

                        queryBuilder.Append($")");

                        // Add a comma separator for all lines except the last one
                        if (objRecord != records.Last())
                        {
                            queryBuilder.Append(", ");
                        }
                    }
                }
            }

            queryBuilder.Append(");");
            return queryBuilder.ToString();
        }
    }
}
