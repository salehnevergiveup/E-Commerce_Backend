namespace PototoTrade.DTO.Wallet
{
    public class RefundRequestDTO
    {
        public int RefundRequestId {get; set;}
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal Amount { get; set; }
        public string Status {get; set;} = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}