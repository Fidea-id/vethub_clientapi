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
using static Dapper.SqlMapper;
using Domain.Entities.DTOs;

namespace Infrastructure.Repositories
{
    public class EventLogRepository : GenericRepository<EventLogs, EventLogFilter>, IEventLogRepository
    {
        public EventLogRepository(IDBFactory context) : base(context)
        {
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

        public async Task<DataResultDTO<EventLogs>> GetEventLogByObjectId(string dbName, int recordId, string objectName)
        {
            var filter = new EventLogFilter()
            {
                RecordId = recordId,
                ObjectName = objectName,
            };
            var data = await base.GetByFilter(dbName, filter);
            return data;
        }

        public async Task<DataResultDTO<EventLogs>> GetEventLogByObjectUser(string dbName, int userId, string objectName)
        {
            var filter = new EventLogFilter()
            {
                UserId = userId,
                ObjectName = objectName,
            };
            var data = await base.GetByFilter(dbName, filter);
            return data;
        }
    }
}
