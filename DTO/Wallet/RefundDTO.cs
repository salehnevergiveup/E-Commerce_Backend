namespace PototoTrade.DTO.Wallet
{
    public class RefundDTO
    {
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
        public decimal Amount { get; set; }
    }
}