using DevExpress.XtraReports.Native;
using Domain.Entities;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;
using Infrastructure.Data;
using Infrastructure.Utils;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;
using System.Xml.Linq;
using Dapper;
using Domain.Entities.DTOs;
using FluentEmail.Core;
using System.Reflection;
using Newtonsoft.Json;
using Domain.Entities.Models.Masters;
using System.Security.AccessControl;
using Domain.Entities.Responses.Clients;

namespace Infrastructure.Repositories
{
    public class EventLogRepository : GenericRepository<EventLogs, EventLogFilter>, IEventLogRepository
    {
        public EventLogRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<EventLogs> AddErrorEventLogByParams(string dbName, string objectName, Exception ex, int userId = 0, int recordId = 0)
        {
            var methodName = ex.Source ?? "-";
            var exString = JsonConvert.SerializeObject(ex, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var entity = new EventLogs()
            {
                UserId = userId,
                RecordId = recordId,
                MethodName = methodName,
                ObjectName = objectName,
                Detail = exString,
                MethodType = "Error",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            if (objectName != "Notifications")
            {
                var newId = await base.Add(dbName, entity);
                entity.Id = newId;
            }
            return entity;
        }

        public async Task<EventLogs> AddEventLogByParams(string dbName, int userId, int recordId, string methodName, MethodType methodType, string objectName, string? detail = null)
        {
            var methodTypeResult = FormatUtil.MethodTypeString(methodType);

            var entity = new EventLogs()
            {
                UserId = userId,
                RecordId = recordId,
                MethodName = methodName,
                ObjectName = objectName,
                Detail = detail,
                MethodType = methodTypeResult,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            if(objectName != "Notifications")
            {
                var newId = await base.Add(dbName, entity);
                entity.Id = newId;
            }
            return entity;
        }

        public async Task<DataResultDTO<EventLogs>> GetEventLogByObjectId(string dbName, int recordId, string objectName, string methodName)
        {
            var filter = new EventLogFilter()
            {
                RecordId = recordId,
                ObjectName = objectName,
                MethodName = methodName
            };
            var data = await base.GetByFilter(dbName, filter);
            return data;
        }

        public async Task<DataResultDTO<EventLogs>> GetEventLogByObjectUser(string dbName, int userId, string objectName, string methodName)
        {
            var filter = new EventLogFilter()
            {
                UserId = userId,
                ObjectName = objectName,
                MethodName= methodName
            };
            var data = await base.GetByFilter(dbName, filter);
            return data;
        }

        public async Task<IEnumerable<EventLogs>> GetSendbackLog(string dbName, int recordId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            var data = await _db.QueryAsync<EventLogs>($"SELECT * FROM EventLogs WHERE RecordId = @Id AND IsActive = 1 AND MethodName = 'ChangeAppointmentStatus' AND Detail = 'Update Status to: 3'", new { Id = recordId });
            return data;
        }
    }
}
