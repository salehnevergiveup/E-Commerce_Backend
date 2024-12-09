using PotatoTrade.DTO.Notification;
using PototoTrade.Models.Notification;
using PototoTrade.Models.User;

namespace PotatoTrade.Repository.Notification{
    public interface NotificationRepository{
        Task CreateNotification(Notifications notification);

        Task<bool> CreateBroadcastUserNotification(int roleId, int notificationid);

        Task<List<UserNotificationWithMetadataDTO>> GetLatestNotificationsByUserId(int userId);

        Task<List<UserNotificationWithMetadataDTO>> GetNotificationsForUserAsync(int userId);

        

    }
}