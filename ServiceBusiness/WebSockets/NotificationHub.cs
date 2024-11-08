// Hubs/NotificationHub.cs
//this implemetations is  based on chatgpt and not tested yet 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Sends a notification to a specific user.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="notification">The notification content.</param>
    public async Task SendNotification(string userId, string notification)
    {
        await Clients.Group($"User-{userId}").SendAsync("ReceiveNotification", notification);
    }
}
