using Dapper;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T>: IGenericRepository<T> where T : class
    {
        private readonly IDBFactory _dbFactory;
        private readonly string _tableName;
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

        public async Task<IEnumerable<T>> GetByFilter(string dbName, Dictionary<string, object> filters)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            // Build the WHERE clause based on the filters
            var whereClause = "";
            var parameters = new DynamicParameters();
            foreach (var filter in filters)
            {
                var paramName = $"@{filter.Key}";
                whereClause += $"{filter.Key} = {paramName} AND ";
                parameters.Add(paramName, filter.Value);
            }
            // Remove the trailing "AND " from the where clause
            if (!string.IsNullOrEmpty(whereClause))
            {
                whereClause = "WHERE " + whereClause.TrimEnd("AND ".ToCharArray());
            }
            var query = $"SELECT * FROM {_tableName} {whereClause}";
            return await _db.QueryAsync<T>(query, parameters);
        }

        public async Task Add(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = GetPropertyNames(entity);
            var columnNames = string.Join(", ", propertyNames);
            var valuePlaceholders = string.Join(", ", propertyNames.Select(name => "@" + name));

            var query = $"INSERT INTO {_tableName} ({columnNames}) VALUES ({valuePlaceholders})";

            await _db.ExecuteAsync(query, entity);
        }

        public async Task Update(string dbName, T entity)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = GetPropertyNames(entity);
            var setClause = string.Join(", ", propertyNames.Select(name => $"{name} = @{name}"));

            var query = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

            await _db.ExecuteAsync(query, entity);
        }

        public async Task Delete(string dbName, int id)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            await _db.ExecuteAsync($"DELETE FROM {_tableName} WHERE Id = @Id", new { Id = id });
        }

        private static IEnumerable<string> GetPropertyNames(T entity)
        {
            return entity.GetType().GetProperties().Select(property => property.Name);
        }

        public async Task<IEnumerable<T>> GetAllActive(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<T>($"SELECT * FROM {_tableName} where IsActive = true");
        }
    }
}
