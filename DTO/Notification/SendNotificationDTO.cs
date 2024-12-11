namespace PotatoTrade.DTO.Notification{
    public class SendNotificationDTO
    {
        public string SenderUsername {get; set;} = null!;
        public string Title { get; set; } = null!;

        public string MessageText { get; set; } = null!;

    }

}
