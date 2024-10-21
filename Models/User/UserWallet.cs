using System;
using System.Collections.Generic;

namespace PototoTrade.Models;

public partial class UserWallet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal AvailableBalance { get; set; }

    public decimal OnHoldBalance { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserAccount User { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
