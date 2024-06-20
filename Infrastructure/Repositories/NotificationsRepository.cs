using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class NotificationsRepository : GenericRepository<Notifications, NotificationsFilter>, INotificationsRepository
    {
        public NotificationsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<IEnumerable<Notifications>> TakeRecent(string dbName, int profileId)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<Notifications>($"SELECT * FROM Notifications WHERE ProfileId = @profile ORDER BY CreatedAt DESC LIMIT 5", new { profile = profileId });
        }
    }
}
