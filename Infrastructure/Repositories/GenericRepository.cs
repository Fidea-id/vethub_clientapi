using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Utils;
using System.Data;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T, TFilter> : IGenericRepository<T, TFilter> where T : class where TFilter : BaseEntityFilter
    {
        protected readonly IDBFactory _dbFactory;
        protected readonly string _tableName;
        public GenericRepository(IDBFactory dbFactory)
        {
            _dbFactory = dbFactory;
            _tableName = typeof(T).Name;
        }
        public async Task<T> GetById(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {_tableName} WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<T>> GetAll(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<T>($"SELECT * FROM {_tableName}");
        }
        public async Task<IEnumerable<T>> GetAllActive(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<T>($"SELECT * FROM {_tableName} where IsActive = true");
        }

        public async Task<IEnumerable<T>> GetByFilter(string dbName, TFilter filters)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            // Build the WHERE clause based on the filters
            var whereClause = "";
            var parameters = new DynamicParameters();
            // Get the properties of the filter object
            var filterProperties = typeof(TFilter).GetProperties();

            foreach (var property in filterProperties)
            {
                var value = property.GetValue(filters); 
                if (value != null && property.Name != "SortProp" && property.Name != "SortMode" && property.Name != "Skip" && property.Name != "Take")
                {
                    var paramName = $"@{property.Name}";
                    var propertyType = property.PropertyType;

                    if (propertyType == typeof(int) || propertyType == typeof(bool))
                    {
                        whereClause += $"{property.Name} = {paramName} AND ";
                        parameters.Add(paramName, value);
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        whereClause += $"{property.Name} = {paramName} AND ";
                        parameters.Add(paramName, value, DbType.DateTime);
                    }
                    else
                    {
                        whereClause += $"{property.Name} LIKE {paramName} AND ";
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
                sortClause = $"ORDER BY {filters.SortProp} {sortMode}";
            }

            var limitClause = "";
            if (filters.Skip.HasValue && filters.Take.HasValue)
            {
                limitClause = $"LIMIT {filters.Skip.Value}, {filters.Take.Value}";
            }
            else if (filters.Take.HasValue)
            {
                limitClause = $"LIMIT {filters.Take.Value}";
            }

            var query = $"SELECT * FROM {_tableName} {whereClause} {sortClause} {limitClause}";
            return await _db.QueryAsync<T>(query, parameters);
        }

        public async Task<int> Add(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = CreateTableQueryGenerator.GetPropertyNames(entity);

            var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
            var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames}); SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(query, entity);
        }

        public async Task AddRange(string dbName, IEnumerable<T> entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var query = "";
            foreach (var item in entity)
            {
                var propertyNames = CreateTableQueryGenerator.GetPropertyNames(entity);
                var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                var subquery = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames}) ";
                query += subquery;
            }

            await _db.ExecuteAsync(query, entity);
        }

        public async Task Update(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = CreateTableQueryGenerator.GetPropertyNames(entity);
            var setClause = string.Join(", ", propertyNames.Select(name => $"{name.Name} = @{name.Name}"));

            var query = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

            await _db.ExecuteAsync(query, entity);
        }
        public async Task UpdateRange(string dbName, IEnumerable<T> entity)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = "";

                foreach (var item in entity)
                {
                    var propertyNames = CreateTableQueryGenerator.GetPropertyNames(item);
                    var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

                    var subquery = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id ";
                    query += subquery;
                }

                await _db.ExecuteAsync(query, entity);
            }
        }

        public async Task Remove(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            await _db.ExecuteAsync($"DELETE FROM {_tableName} WHERE Id = @Id", new { Id = id });
        }

        public async Task RemoveRange(string dbName, IEnumerable<T> entity)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var query = "";

                foreach (var item in entity)
                {
                    var propertyNames = CreateTableQueryGenerator.GetPropertyNames(item);
                    var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

                    var subquery = $"DELETE FROM {_tableName} WHERE Id = @Id ";
                    query += subquery;
                }

                await _db.ExecuteAsync(query, entity);
            }
        }

        public async Task<int> CountWithFilter(string dbName, TFilter filters)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var whereClause = "";
                var parameters = new DynamicParameters();

                var filterProperties = typeof(TFilter).GetProperties();

                foreach (var property in filterProperties)
                {
                    var value = property.GetValue(filters);
                    if (value != null && property.Name != "SortProp" && property.Name != "SortMode")
                    {
                        var paramName = $"@{property.Name}";
                        whereClause += $"{property.Name} = {paramName} AND ";
                        parameters.Add(paramName, value);
                    }
                }

                if (!string.IsNullOrEmpty(whereClause))
                {
                    whereClause = "WHERE " + whereClause.TrimEnd("AND ".ToCharArray());
                }

                var query = $"SELECT COUNT(*) FROM {_tableName} {whereClause}";
                return await _db.ExecuteScalarAsync<int>(query, parameters);
            }
        }
    }
}
