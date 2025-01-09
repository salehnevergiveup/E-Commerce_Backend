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

     public async Task<List<Get5LatestNotificationDTO>> GetLatestNotificationsByUserId(int userId)
    {
        try
        {
            // Fetch the latest 5 notifications tied to the specified userId
            var latestNotifications = await (
                from notification in _context.Notifications
                join userNotification in _context.UserNotification
                on notification.Id equals userNotification.NotificationId into userNotificationsGroup
                from userNotification in userNotificationsGroup.DefaultIfEmpty() // Perform left join

                where 
                    notification.ReceiverId == userId || (userNotification != null && userNotification.UserId == userId)
                orderby notification.CreatedAt descending // Order by creation date

                select new
                {
                    notification,
                    userNotification
                }
            )
            .GroupBy(n => n.notification.Id) // Group by notification ID to avoid duplicates
            .Select(group => new Get5LatestNotificationDTO
            {
                UserUsername = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.UserUsername : null,
                SenderUsername = group.FirstOrDefault().notification.SenderUsername,
                NotificationId = group.Key, // Grouped notification ID
                Title = group.FirstOrDefault().notification.Title,
                MessageText = group.FirstOrDefault().notification.MessageText,
                CreatedAt = group.FirstOrDefault().notification.CreatedAt,
                Type = group.FirstOrDefault().notification.Type,
                IsRead = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.IsRead : null,
                UpdatedAt = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.UpdatedAt : null,
                ReceiverUsername = group.FirstOrDefault().notification.ReceiverUsername,
                Status = group.FirstOrDefault().notification.Status
            })
            .OrderByDescending(dto => dto.CreatedAt) // Order by the creation timestamp
            .Take(5) // Limit to the latest 5 notifications
            .ToListAsync();

            return latestNotifications;
        }
        catch (Exception e)
        {
            return null;
        }
    }


       public async Task<List<GetAllLatestNotificationsDTO>> GetNotificationsForUserAsync(int userId)
        {
            try
            {
                var notifications = await (
                    from notification in _context.Notifications
                    join userNotification in _context.UserNotification
                    on notification.Id equals userNotification.NotificationId into userNotificationsGroup
                    from userNotification in userNotificationsGroup.DefaultIfEmpty() // Perform left join

                    where 
                        notification.ReceiverId == userId || (userNotification != null && userNotification.UserId == userId)
                    orderby notification.CreatedAt descending

                    select new
                    {
                        notification,
                        userNotification
                    }
                )
                .GroupBy(n => n.notification.Id) // Group by notification ID to avoid duplicates
                .Select(group => new GetAllLatestNotificationsDTO
                {
                    SenderUsername = group.FirstOrDefault().notification.SenderUsername,
                    ReceiverUsername = group.FirstOrDefault().notification.ReceiverUsername,
                    UserUsername = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.UserUsername : null,
                    NotificationId = group.Key, // Grouped notification ID
                    Title = group.FirstOrDefault().notification.Title,
                    MessageText = group.FirstOrDefault().notification.MessageText,
                    CreatedAt = group.FirstOrDefault().notification.CreatedAt,
                    Type = group.FirstOrDefault().notification.Type,
                    IsRead = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.IsRead : null,
                    UpdatedAt = group.FirstOrDefault().userNotification != null ? group.FirstOrDefault().userNotification.UpdatedAt : null,
                    Status = group.FirstOrDefault().notification.Status
                })
                .OrderByDescending(dto => dto.CreatedAt)
                .ToListAsync();

                return notifications;
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }



        public async Task<List<UserNotification>> GetUnreadNotificationsForUserAsync(int userId)
        {
            var unreadNotifications = await _context.UserNotification
        .Where(n => n.UserId == userId && !n.IsRead)
        .ToListAsync();

        return unreadNotifications;
        }

     public async Task<int> MarkNotificationsAsReadAsync(int userId)
{
    try
    {
        // Retrieve all UserNotification entries for the user
        var userNotifications = await _context.UserNotification
            .Where(un => un.UserId == userId && !un.IsRead) // Get only unread notifications
            .Include(un => un.Notification) // Include related Notification data
            .ToListAsync();

        Console.WriteLine($"Number of UserNotifications fetched: {userNotifications.Count}");

        // Update UserNotification entries
        foreach (var userNotification in userNotifications)
        {
            Console.WriteLine($"UserNotification ID: {userNotification.UserNotificationId}, IsRead: {userNotification.IsRead}, NotificationId: {userNotification.NotificationId}");

            // Update the IsRead field for the UserNotification model
            userNotification.IsRead = true;
            userNotification.UpdatedAt = DateTime.UtcNow;

            // Update the Status field for the related Notification model
            if (userNotification.Notification != null)
            {
                _context.Attach(userNotification.Notification); // Ensure Notification is tracked
                userNotification.Notification.Status = "Read";

                Console.WriteLine($"Notification ID {userNotification.Notification.Id} updated to Status: Read");
            }
        }

        // Handle standalone notifications without UserNotification entries
        var standaloneNotifications = await _context.Notifications
            .Where(n => !_context.UserNotification.Any(un => un.NotificationId == n.Id) && n.Status != "Read")
            .ToListAsync();

        Console.WriteLine($"Number of standalone Notifications fetched: {standaloneNotifications.Count}");

        foreach (var standaloneNotification in standaloneNotifications)
        {
            standaloneNotification.Status = "Read";
            _context.Attach(standaloneNotification);

            Console.WriteLine($"Standalone Notification ID {standaloneNotification.Id} updated to Status: Read");
        }

        // Save changes to the database
        int rowsAffected = await _context.SaveChangesAsync();
        Console.WriteLine($"Rows affected: {rowsAffected}");

        return rowsAffected; // Returns the number of rows updated
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
        return 0; // Indicate failure
    }
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

