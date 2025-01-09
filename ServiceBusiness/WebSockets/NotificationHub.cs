using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.Repository.Notification;
using PotatoTrade.Service.Notification;
using PototoTrade.Service.Utilities.Response;
using System.Security.Claims;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    private readonly NotificationRepository _notificationRepository;

    private readonly NotificationService _notificationService;

    public NotificationHub(NotificationRepository notificationRepository, NotificationService notificationService)
    {
        _notificationRepository = notificationRepository;
        _notificationService = notificationService;
    }

    [Authorize(Roles = "Admin,User")]
    public override async Task OnConnectedAsync()
    {
        var userId = int.Parse(Context.User.FindFirst(ClaimTypes.Name)?.Value);
        var userRole = Context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (userId != 0)
        {
            // Add user to individual group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
            Console.WriteLine($"User with ID {userId} connected and added to group: User-{userId}");

            // Add user to role-specific group
            if (userRole == "User")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
                Console.WriteLine($"User with ID {userId} added to group: Users");
            }

            if (userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                Console.WriteLine($"User with ID {userId} added to group: Admins");
            }

            // Fetch and send the latest notifications
            var latestNotifications = await _notificationRepository.GetLatestNotificationsByUserId(userId);

            Console.WriteLine($"Fetched latest notifications for user ID: {userId}");

            if (latestNotifications != null)
            {
                foreach (var notification in latestNotifications)
                {
                    Console.WriteLine($"INITIAL Notification ID: {notification.NotificationId}, Title: {notification.Title}, Message: {notification.MessageText}, Created At: {notification.CreatedAt}, IsRead: {notification.IsRead}");
                }
                await Clients.Group($"User-{userId}").SendAsync("ReceiveListofLatestNotification", latestNotifications);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        var userRole = Context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Remove user from individual group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
            Console.WriteLine($"User with ID {userId} disconnected and removed from group: User-{userId}");

            // Remove user from role-specific group
            if (userRole == "User")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Users");
                Console.WriteLine($"User with ID {userId} removed from group: Users");
            }

            if (userRole == "Admin")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                Console.WriteLine($"User with ID {userId} removed from group: Admins");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}


    // public async Task<ResponseModel<bool>> SendNotificationToUsers(string notification) //for users
    // {
    //     var response  = new ResponseModel<bool>{
    //             Success = false,
    //             Data = false,
    //             Message = "Failed to send broadcast via hub"
    //         };
    //     try{
    //         await Clients.Group("Users").SendAsync("ReceiveNotification", notification);

    //         response.Success = true;
    //         response.Data = true;
    //         response.Message = "broadcasted message from hub successfully";
            
    //     }catch (Exception e){
    //         response.Message = $"An error occurred: {e.Message}";
    //     }
    //     return response;

    // }

