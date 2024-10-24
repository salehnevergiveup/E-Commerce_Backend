using Microsoft.AspNetCore.SignalR;
using PototoTrade.Models;
//using SignalR.DataService;
//using SignalR.Models;
using System.Collections.Generic;

namespace PototoTrade.Service.User
{
    public class ChatHub : Hub
    {   private readonly SharedDb _shared;

        public ChatHub(SharedDb shared) => _shared = shared;

        //public async Task GetAllConnections()
        //{
        //    var allConnections = _shared.connections;
        //    foreach (var connection in allConnections)
        //    {
        //        Console.WriteLine($"ConnectionId: {connection.Key}, Username: {connection.Value.Username}, Chatroom: {connection.Value.Chatroom}");

        //    }
        //}
        public override async Task OnConnectedAsync()
{
        // Add the new connection to the dictionary
        _shared.connections[Context.ConnectionId] = new UserAccount
        {
            Username = "Anonymous", // Optional: Set a placeholder until the user joins a chat
            Chatroom = "None"
        };

        Console.WriteLine($"Connection added: {Context.ConnectionId}");

        await base.OnConnectedAsync();
        await GetActiveConnectionsCount(); // Update the active connection count
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove the disconnected connection from the dictionary
        bool removed = _shared.connections.TryRemove(Context.ConnectionId, out var removedConnection);

        if (removed)
        {
            Console.WriteLine($"Connection removed: {Context.ConnectionId}");
        }
        else
        {
            Console.WriteLine($"Failed to remove connection: {Context.ConnectionId}");
        }

        await base.OnDisconnectedAsync(exception);
        await GetActiveConnectionsCount(); // Update the count after disconnection
    }

    public async Task<int> GetActiveConnectionsCount()
    {
        int activeConnections = _shared.connections.Count;
        Console.WriteLine($"Active connections: {activeConnections}");

        // Broadcast the active connection count to all connected clients
        await Clients.All.SendAsync("UpdateConnectionsCount", activeConnections);
        return activeConnections;
    }

        public async Task JoinChat(UserAccount conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined");
        }
        public async Task JoinSpecificChatRoom(UserAccount conn) //for when buyer click "chat now" with seller
        {
            Console.WriteLine($"User {conn.Username} is joining {conn.Chatroom}"); // Log for debugging

            await Groups.AddToGroupAsync(Context.ConnectionId, conn.Chatroom);
            _shared.connections[Context.ConnectionId] = conn;
            await Clients.Group(conn.Chatroom).SendAsync("ReceiveMessage", "admin", $"{conn.Username} has joined {conn.Chatroom}", DateTime.Now.ToString("h:mm tt"));
        }
        
        public async Task SendMessage(string message)
        {
            if (_shared.connections.TryGetValue(Context.ConnectionId, out UserAccount conn))
            {
                await Clients.Group(conn.Chatroom).SendAsync("ReceiveMessage", conn.Username, message, DateTime.Now.ToString("h:mm tt"));
            }
        }
    }
}
