using PotatoTrade.DTO.Notification;
using PototoTrade.Data;
using PototoTrade.Models.Notification;
using PototoTrade.Models.User;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Repository.Users;


namespace PotatoTrade.Repository.Notification
{
    public class NotificationRepositoryImpl : NotificationRepository
    {
        private readonly DBC _context;
        private readonly UserAccountRepository _userAccountRepository;

        public NotificationRepositoryImpl(DBC context, UserAccountRepository userAccountRepository)
        {
            _context = context;
            _userAccountRepository = userAccountRepository;
        }

        public async Task CreateNotification(Notifications newNotification)
        {
            try{
                await _context.Notifications.AddAsync(newNotification);
                await _context.SaveChangesAsync();
            }catch(Exception e){
                Console.WriteLine("create new notification error" + e);
            }
            
        }

        public async Task<bool> CreateBroadcastUserNotification(int roleId, int notificationId)
        {
            try
            {
                 var userResponse = await _userAccountRepository.GetUserIdsAndUsernamesByRoleId(roleId);

                if (userResponse == null || userResponse.Count == 0)
                {
                    // Handle the failure case
                    return false;
                }
                var userNotifications = userResponse.Select(user => new UserNotification
                {
                    UserId = user.UserId,
                    UserUsername = user.Username,
                    NotificationId = notificationId,
                    IsRead = false,
                    ReceivedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.UserNotification.AddRangeAsync(userNotifications);
                await _context.SaveChangesAsync();

                return true; // Return success status
            }
            catch (Exception)
            {
                return false; // Return failure status
            }
            
        }

        public async Task<List<UserNotificationWithMetadataDTO>> GetLatestNotificationsByUserId(int userId)
        {
            try
        {
            var latestNotifications = await (from notification in _context.Notifications
                                            join userNotification in _context.UserNotification
                                            on notification.Id equals userNotification.NotificationId
                                            where userNotification.UserId == userId
                                            orderby notification.CreatedAt descending
                                            select new UserNotificationWithMetadataDTO
                                            {
                                                UserUsername = userNotification.UserUsername,
                                                SenderUsername = notification.SenderUsername,
                                                NotificationId = userNotification.NotificationId,
                                                Title = notification.Title,
                                                MessageText = notification.MessageText,
                                                CreatedAt = notification.CreatedAt,
                                                Type = notification.Type,
                                                IsRead = userNotification.IsRead,
                                                UpdatedAt = userNotification.UpdatedAt
                                            }).Take(5).ToListAsync();

            return latestNotifications;
        }
        catch (Exception e)
        {
            return null;
        }
        
        }

        public async Task<List<UserNotificationWithMetadataDTO>> GetNotificationsForUserAsync(int userId)
        {
            return await (
            from notification in _context.Notifications
            join userNotification in _context.UserNotification
            on notification.Id equals userNotification.NotificationId
            where userNotification.UserId == userId
            orderby notification.CreatedAt descending
            select new UserNotificationWithMetadataDTO
            {
                SenderUsername = notification.SenderUsername,
                UserUsername = userNotification.UserUsername,
                NotificationId = notification.Id,
                Title = notification.Title,
                MessageText = notification.MessageText,
                CreatedAt = notification.CreatedAt,
                Type = notification.Type,
                IsRead = userNotification.IsRead,
                UpdatedAt = userNotification.UpdatedAt
            }
        ).ToListAsync();
        }

        public async Task<List<UserNotification>> GetUnreadNotificationsForUserAsync(int userId)
        {
            var unreadNotifications = await _context.UserNotification
        .Where(n => n.UserId == userId && !n.IsRead)
        .ToListAsync();

        return unreadNotifications;
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(List<UserNotification> notifications)
        {
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow; // Update the timestamp
            }

            return await _context.SaveChangesAsync(); // Return the number of rows updated
        }

        public async Task<List<NotificationDTO>> GetAllNotificationsForAdmin(){
        try
        {
        // Fetch notifications along with the read/unread counts
        var notifications = await (
            from notification in _context.Notifications
            join userNotification in _context.UserNotification
            on notification.Id equals userNotification.NotificationId into userNotificationsGroup
            where notification.ReceiverUsername == "all" && notification.Type == "broadcast"
            select new NotificationDTO
            {
                SenderUsername = notification.SenderUsername,
                ReceiverUsername = null, // Admins do not need receiver info
                Type = notification.Type,
                Title = notification.Title,
                MessageText = notification.MessageText,
                CreatedAt = notification.CreatedAt,
                Status = notification.Status,
                // Aggregate read/unread counts
                ReadCount = userNotificationsGroup.Count(ur => ur.IsRead),
                TotalCount = userNotificationsGroup.Count()
            }
        ).OrderByDescending(n => n.CreatedAt).ToListAsync();

        return notifications;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notifications for admin: {ex.Message}");
            return new List<NotificationDTO>();
        }
        }

        // public async Task<SendNotificationDTO> GetSendNotificationDTOByNotificationId(int notificationId){
        //     try 
        // }

        // public async Task<List<UserNotification>> MarkAllNotificationsAsRead(int userId, UserNotification userNotifications)
        // {
        //     return await
        // }


    }
}

