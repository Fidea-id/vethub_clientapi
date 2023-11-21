using Domain.Entities.Requests.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
