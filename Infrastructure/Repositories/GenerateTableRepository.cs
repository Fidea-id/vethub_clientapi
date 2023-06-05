using Dapper;
using Domain.Entities.Models;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Repositories
{
    public class GenerateTableRepository : IGenerateTableRepository
    {
        private readonly IDBFactory _dbFactory;
        public GenerateTableRepository(IDBFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task GenerateAllTable(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName, true);

            var models = new List<Type>
            {
                typeof(Profile),
                // Add more model types here
            };
            foreach (var modelType in models)
            {
                string createTableQuery = CreateTableQueryGenerator.GenerateCreateTableQuery(modelType);
                await _db.ExecuteAsync(createTableQuery);
            }
        }
    }
}
