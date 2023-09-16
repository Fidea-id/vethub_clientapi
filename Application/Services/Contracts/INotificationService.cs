using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;

namespace Application.Services.Contracts
{
    public interface INotificationService : IGenericService<Notifications, NotificationsRequest, Notifications, NotificationsFilter>
    {
        Task<IEnumerable<Notifications>> GetRecent(string dbName, int profile);
        Task<DataResultDTO<Notifications>> GetAll(string dbName, int profile);
        Task ReadAllNotification(string dbName, int profile);
        Task CreateNotification(string dbName, NotificationsRequest request);
        Task ReadNotificationById(string dbName, int profile, int id);

    }
}
