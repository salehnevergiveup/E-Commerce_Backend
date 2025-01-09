namespace PotatoTrade.DTO.Notification{
    public class OrderPurchasedNotificationDTO
    {
       public int NotificationId { get; set; }
        public string SenderUsername {get; set;} = "System";
        public string ReceiverUsername {get; set;} = null!;

        public string Type {get; set;} = "item purchased notification";
        public string Title { get; set; } = null!;
        public bool IsRead { get; set; }

        public string MessageText { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt {get; set;}

        public string Status { get; set; } = "notRead";
    }

}
