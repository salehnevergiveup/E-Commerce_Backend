namespace PotatoTrade.DTO.Notification{
    public class OrderPurchasedNotificationDTO
    {
        public string SenderUsername {get; set;} = "auto-generated";
        public string ReceiverUsername {get; set;} = null!;

        public string Type {get; set;} = "item purchased notification";
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = "notRead";
    }

}
