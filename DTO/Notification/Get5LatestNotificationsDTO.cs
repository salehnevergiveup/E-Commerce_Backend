using Microsoft.Identity.Client;

namespace PotatoTrade.DTO.Notification{

public class Get5LatestNotificationDTO
{
    public string SenderUsername { get; set;} 
    public string ReceiverUsername {get; set;}
    public string UserUsername { get; set;} 
    public int NotificationId { get; set; }
    public string Title { get; set; }
    public string MessageText { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Status {get; set;}
}

}
