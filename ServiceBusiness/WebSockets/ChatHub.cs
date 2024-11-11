// Hubs/ChatHub.cs
//this code is provided by the chatgpt and not tested yet
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
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
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message content.</param>
    public async Task SendMessage(string message)
    {
        var user = Context.User.Identity.Name;
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    /// <summary>
    /// Sends a private message to a specific user.
    /// </summary>
    /// <param name="targetUserId">The target user's ID.</param>
    /// <param name="message">The message content.</param>
    public async Task SendPrivateMessage(string targetUserId, string message)
    {
        var sender = Context.User.Identity.Name;
        await Clients.Group($"User-{targetUserId}").SendAsync("ReceivePrivateMessage", sender, message);
    }
}
