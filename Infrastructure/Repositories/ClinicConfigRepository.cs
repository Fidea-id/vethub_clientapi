using Dapper;
using Domain.Entities.Filters;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Infrastructure.Repositories
{
    public class ClinicConfigRepository : GenericRepository<ClinicConfig, NameBaseEntityFilter>, IClinicConfigRepository
    {
        public ClinicConfigRepository(IDBFactory context) : base(context)
        {
        }
        public async Task<int> AddConfig(string dbName, ClinicConfig config)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var query = "INSERT INTO ClinicConfig (`Key`, `Value`, Id, IsActive, CreatedAt, UpdatedAt) VALUES (@Key, @Value, @Id, @IsActive, @CreatedAt, @UpdatedAt)";

            var parameters = new
            {
                Key = config.Key,
                Value = config.Value,
                Id = config.Id,
                IsActive = config.IsActive,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt
            };
            await _db.ExecuteAsync(query, parameters);

            return await _db.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID();");
        }
        public async Task UpdateConfig(string dbName, ClinicConfig config)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var query = "UPDATE ClinicConfig SET `Value` = @Value, IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE `Key` = @Key AND Id = @Id";
            var parameters = new
            {
                Value = config.Value,
                IsActive = config.IsActive,
                UpdatedAt = config.UpdatedAt,
                Key = config.Key,
                Id = config.Id
            };
            await _db.ExecuteAsync(query, parameters);
        }

        public async Task<string> GetConfigValue(string dbName, string key)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var value = await _db.QueryFirstOrDefaultAsync<string>($"SELECT Value FROM ClinicConfig WHERE `Key` = @Key", new { Key = key });
            return value;
        }
        public async Task<ClinicConfig> GetConfigByKey(string dbName, string key)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var value = await _db.QueryFirstOrDefaultAsync<ClinicConfig>($"SELECT * FROM ClinicConfig WHERE `Key` = @Key", new { Key = key });
            return value;
        }
    }
}
