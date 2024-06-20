using Domain.Entities.Requests.Clients;

namespace Application.Utils
{
    public static class NotificationUtil
    {
        public static NotificationsRequest SetCreateNotifRequest(int id, string title, string message, string url)
        {
            var baseUrl = "https://app.vethub.id/";
            var notif = new NotificationsRequest(id, "Create", title, message, baseUrl + url);
            return notif;
        }
        public static NotificationsRequest SetUpdateNotifRequest(int id, string title, string message, string url)
        {
            var baseUrl = "https://app.vethub.id/";
            var notif = new NotificationsRequest(id, "Update", title, message, baseUrl + url);
            return notif;
        }
        public static NotificationsRequest SetDeleteNotifRequest(int id, string title, string message)
        {
            var notif = new NotificationsRequest(id, "Delete", title, message, null);
            return notif;
        }
    }
}
