// Hubs/NotificationHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.Repository.Notification;
using PototoTrade.Service.Utilities.Response;
using System.Security.Claims;
using System.Threading.Tasks;


public class NotificationHub : Hub
{
    private readonly NotificationRepository _notificationRepository;

    public NotificationHub(NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [Authorize(Roles = "User")]
     public override async Task OnConnectedAsync()
    {
        var userId = int.Parse(Context.User.FindFirst(ClaimTypes.Name)?.Value);
        var userRole = Context.User.FindFirst(ClaimTypes.Role)?.Value;


        if (userId != 0)
        {
            // Add user to individual group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
            // Add user to role-specific group
            if (userRole == "3") // Assuming "3" represents RoleId for regular users
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
            }
            var latestNotifications = await _notificationRepository.GetLatestNotificationsByUserId(userId);
            Console.WriteLine("Unread Notifications fetched for user ID: " + userId);

            if (latestNotifications != null)
            {
                foreach (var notification in latestNotifications)
            {
                Console.WriteLine($"Notification ID: {notification.NotificationId}, Title: {notification.Title}, Message: {notification.MessageText}, Created At: {notification.CreatedAt}");
            }

                await Clients.Caller.SendAsync("ReceiveListofLatestNotification", latestNotifications);
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

                // Remove user from role-specific group
                if (userRole == "3")
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Users");
                }
            }

            await base.OnDisconnectedAsync(exception);
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
}
