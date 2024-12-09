using System;
using System.Collections.Generic;
using PototoTrade.Models.User;

namespace PototoTrade.Models.Notification;

public partial class Notifications
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public string SenderUsername {get; set;} = null!;

    public int ReceiverId { get; set; } 
    public string ReceiverUsername {get; set;} = null!;
    public string Type { get; set; } = "broadcast"; 
    public string Title { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;

    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
