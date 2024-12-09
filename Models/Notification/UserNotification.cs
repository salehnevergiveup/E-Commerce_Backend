using PototoTrade.Models.Notification;
using PototoTrade.Models.User;

public partial class UserNotification
{
    public int UserNotificationId { get; set; }

    // Foreign Key to the User
    public int UserId { get; set; }
    public string UserUsername { get; set; }
    public virtual UserAccount User { get; set; } = null!;

    // Foreign Key to the Notification
    public int NotificationId { get; set; }
    public virtual Notifications Notification { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime ReceivedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
