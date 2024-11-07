namespace PototoTrade.DTO.Wallet
{
    public class TopUpRequestDTO
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}