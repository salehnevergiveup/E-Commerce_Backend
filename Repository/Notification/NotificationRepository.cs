using PotatoTrade.DTO.Notification;
using PototoTrade.Models.Notification;
using PototoTrade.Models.User;

namespace PotatoTrade.Repository.Notification{
    public interface NotificationRepository{
        Task CreateNotification(Notifications notification);

        Task<bool> CreateBroadcastUserNotification(int roleId, int notificationid);

        Task<List<Get5LatestNotificationDTO>> GetLatestNotificationsByUserId(int userId);

        Task<List<GetAllLatestNotificationsDTO>> GetNotificationsForUserAsync(int userId);
        Task<List<UserNotification>> GetUnreadNotificationsForUserAsync(int userId);

        Task<int> MarkNotificationsAsReadAsync(int userId);

        Task<List<NotificationDTO>> GetAllNotificationsForAdmin();

        //Task<List<SendNotificationDTO>> GetSendNotificationDTOByNotificationId(int notificationId);

        //Task<List<UserNotification>> MarkAllNotificationsAsRead(int userId, UserNotification userNotifications);

        

    }
}