namespace PotatoTrade.DTO.Notification{
    public class SystemInnerNotificationDto
    {
        public string SenderUsername {get; set;} = "System";

        public int senderId {get; set;}
        public string ReceiverUsername {get; set;} = null!;

        public int ReceiverId {get; set;} 
        
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;


    }

}
