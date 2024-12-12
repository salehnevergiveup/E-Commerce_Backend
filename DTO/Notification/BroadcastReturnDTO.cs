namespace PotatoTrade.DTO.Notification{
    public class BroadcastReturnDTO
    {
       public int NotificationId { get; set; }

        public string SenderUsername {get; set;} = null!;
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public DateTime UpdatedAt {get; set;}

        public string UserUsername {get; set;}

        public string Type {get; set;}
        


    }

}
