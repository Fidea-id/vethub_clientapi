using Dapper;
using Domain.Entities;
using Domain.Entities.DTOs;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        public async Task<DataResultDTO<T>> GetByFilter(string dbName, TFilter filters)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filterQuery = QueryGenerator.GenerateFilterQuery(filters, _tableName);
            var queryString = filterQuery.Item1;
            var countQuery = QueryGenerator.GenerateSelectOrCountQuery(queryString, true);
            var countData = await _db.QueryFirstOrDefaultAsync<int>(countQuery, filterQuery.Item2);
            if (filters.Take.HasValue || filters.Skip.HasValue)
            {
                queryString = QueryGenerator.GenerateFilteredLimitQuery(queryString, filters.Skip, filters.Take);
            }
            var data = await _db.QueryAsync<T>(queryString, filterQuery.Item2);
            var result = new DataResultDTO<T>
            {
                Data = data,
                TotalData = countData
            };
            return result;
        }

        public async Task<int> Add(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(entity);

            var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
            var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames}); SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(query, entity);
        }

        public async Task AddRange(string dbName, IEnumerable<T> entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            foreach (var item in entity)
            {
                var propertyNames = QueryGenerator.GetPropertyNames(item);
                var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                var subquery = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({parameterNames}) ";
                await _db.ExecuteAsync(subquery, item);
            }
        }

        public async Task Update(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(entity);
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
                    var propertyNames = QueryGenerator.GetPropertyNames(item);
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
                    var propertyNames = QueryGenerator.GetPropertyNames(item);
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

        public async Task<IEnumerable<T>> WhereQuery(string dbName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<T>($"SELECT * FROM {_tableName} WHERE {query}");
        }

        public async Task<T> WhereFirstQuery(string dbName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstAsync<T>($"SELECT * FROM {_tableName} WHERE {query}");
        }
        public async Task<bool> AnyQuery(string dbName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            string querys = $"SELECT COUNT(*) FROM {_tableName} WHERE {query}";
            var result = await _db.ExecuteScalarAsync<int>(querys);
            return result > 0;
        }

        public async Task<int> Count(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var query = $"SELECT COUNT(*) FROM {_tableName}";
            return await _db.ExecuteScalarAsync<int>(query);
        }

        public async Task<int> CountWithQuery(string dbName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {_tableName} WHERE {query}");
        }
        public async Task<CardDashboard> CountToCard(string dbName, string date, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var filter = "";
            if (!string.IsNullOrEmpty(query))
            {
                filter = $"Where {query}";
            }
            if (string.IsNullOrEmpty(date))
            {
                date = $"2023-01-01"; //set min date
            }
            var resultQuery = $"SELECT COUNT(Id) AS TotalAll, COUNT(CASE WHEN CreatedAt > @Date THEN Id END) AS Total FROM {_tableName} {filter}";
            return await _db.QueryFirstAsync<CardDashboard>(resultQuery, new { Date = date });
        }
        public async Task<int> Sum(string dbName, string columnName)
        {
            var _db = _dbFactory.GetDbConnection(dbName); 
            string sql = $"SELECT SUM({columnName}) FROM {_tableName}";
            return await _db.ExecuteScalarAsync<int>(sql);
        }
        public async Task<int> SumWithQuery(string dbName, string columnName, string query)
        {
            var _db = _dbFactory.GetDbConnection(dbName); 
            string sql = $"SELECT SUM({columnName}) FROM {_tableName} WHERE {query}";
            return await _db.ExecuteScalarAsync<int>(sql);
        }
    }
}
