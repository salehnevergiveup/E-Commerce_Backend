namespace PotatoTrade.DTO.Notification{
    public class NotificationDTO
    {
        public int UserId { get; set; }
        public int ReceiverId {get; set;}

        public string Type {get; set;} = "broadcast";
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = "unread";

    }

}
