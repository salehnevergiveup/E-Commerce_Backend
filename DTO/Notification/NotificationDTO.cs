namespace PotatoTrade.DTO.Notification{
    public class NotificationDTO
    {
        public string SenderUsername {get; set;} = null!;
        public string ReceiverUsername {get; set;} = null!;

        public string Type {get; set;} = "broadcast";
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = "sent";

        public int ReadCount {get; set;}
        public int TotalCount {get; set;}

    }

}
