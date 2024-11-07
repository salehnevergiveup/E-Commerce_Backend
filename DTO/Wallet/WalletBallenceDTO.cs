namespace PototoTrade.DTO.Wallet{
    public class WalletBalanceDTO{
        public int UserId { get; set; }
        public decimal AvailableBalance { get; set; }

        public decimal OnHoldBalance {get; set; }
    }
}
