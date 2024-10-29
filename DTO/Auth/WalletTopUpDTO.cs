using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO;

public record class WalletTopUpDTO{
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}