using Application.Services.Contracts;
using Application.Utils;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Interfaces.Clients;

namespace Application.Services.Implementations
{
    public class ServicesService : GenericService<Domain.Entities.Models.Clients.Services, ServicesRequest, Domain.Entities.Models.Clients.Services, ServicesFilter>, IServicesService
    {
        private readonly INotificationService _notificationService;
        public ServicesService(IUnitOfWork unitOfWork, IGenericRepository<Domain.Entities.Models.Clients.Services, ServicesFilter> repository, ICurrentUserService currentUser, INotificationService notificationService)
        : base(unitOfWork, repository, currentUser)
        {
            _notificationService = notificationService;
        }

        public override async Task<Domain.Entities.Models.Clients.Services> CreateRequestAsync(ServicesRequest request, string? dbName)
        {
            var result = await base.CreateRequestAsync(request, dbName);

            var currentUserId = await _currentUser.UserId;

            //create notif
            var url = "services";
            var notif = NotificationUtil.SetCreateNotifRequest(currentUserId, "Create Service", $"Service created", url);
            await _notificationService.CreateRequestAsync(notif, dbName);

            return result;
        }
        public override async Task<Domain.Entities.Models.Clients.Services> UpdateAsync(int id, ServicesRequest request, string? dbName)
        {
            var result = await base.UpdateAsync(id, request, dbName);

            var currentUserId = await _currentUser.UserId;

            //create notif
            var url = "services";
            var notif = NotificationUtil.SetUpdateNotifRequest(currentUserId, "Update Service", $"Service updated", url);
            await _notificationService.CreateRequestAsync(notif, dbName);

            return result;
        }
        public override async Task DeleteAsync(int id, string? dbName)
        {
            await base.DeleteAsync(id, dbName);

            var currentUserId = await _currentUser.UserId;

            //create notif
            var notif = NotificationUtil.SetDeleteNotifRequest(currentUserId, "Delete Service", $"Service deleted");
            await _notificationService.CreateRequestAsync(notif, dbName);
        }
    }
}
