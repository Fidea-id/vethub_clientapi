using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;
using Domain.Utils;

namespace Application.Services.Implementations
{
    public class NotificationService : GenericService<Notifications, NotificationsRequest, Notifications, NotificationsFilter>, INotificationService
    {
        public NotificationService(IUnitOfWork unitOfWork, IGenericRepository<Notifications, NotificationsFilter> repository)
        : base(unitOfWork, repository)
        { }
        public async Task CreateNotification(string dbName, NotificationsRequest request)
        {
            try
            {
                //trim all string
                FormatUtil.TrimObjectProperties(request);
                var entity = Mapping.Mapper.Map<Notifications>(request);
                FormatUtil.SetIsActive<Notifications>(entity, true);
                FormatUtil.SetDateBaseEntity<Notifications>(entity);

                var newId = await _repository.Add(dbName, entity);
                entity.Id = newId;

                //TODO:add push fcm
            }
            catch (Exception ex)
            {
                ex.Source = $"NotificationService.CreateRequestAsync";
                throw;
            }

        }

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
