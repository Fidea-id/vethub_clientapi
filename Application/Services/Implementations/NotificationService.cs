using Application.Services.Contracts;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class NotificationService : GenericService<Notifications, NotificationsRequest, Notifications, NotificationsFilter>, INotificationService
    {
        public NotificationService(IUnitOfWork unitOfWork, IGenericRepository<Notifications, NotificationsFilter> repository)
        : base(unitOfWork, repository)
        { }

        public async Task<DataResultDTO<Notifications>> GetAll(string dbName, int profile)
        {
            try
            {
                var filters = new NotificationsFilter()
                {
                    ProfileId = profile,
                };
                var data = await _repository.GetByFilter(dbName, filters);
                return data;
            }
            catch (Exception ex)
            {
                ex.Source = $"NotificationService.GetAll";
                throw;
            }
        }

        public async Task<IEnumerable<Notifications>> GetRecent(string dbName, int profile)
        {
            try
            {
                var data = await _unitOfWork.NotificationsRepository.TakeRecent(dbName, profile);
                return data; 
            }
            catch (Exception ex)
            {
                ex.Source = $"NotificationService.GetRecent";
                throw;
            }
        }

        public async Task ReadAllNotification(string dbName, int profile)
        {
            try
            {
                var data = await _repository.WhereQuery(dbName, $"ProfileId = {profile} AND IsRead = FALSE");
                foreach(var notification in data)
                {
                    notification.IsRead = true;
                    await _repository.Update(dbName, notification);
                }
            }
            catch (Exception ex)
            {
                ex.Source = $"NotificationService.ReadAllNotification";
                throw;
            }
        }

        public async Task ReadNotificationById(string dbName, int profile, int id)
        {
            try
            {
                var data = await _repository.GetById(dbName, id);
                if (data.ProfileId != profile) throw new Exception("Notification not found");
                data.IsRead = true;
                await _repository.Update(dbName, data);
            }
            catch (Exception ex)
            {
                ex.Source = $"NotificationService.ReadNotificationById";
                throw;
            }
        }
    }
}
