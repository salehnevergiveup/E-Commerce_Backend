using System;
using System.Collections.Generic;

namespace PototoTrade.Models.User;

public partial class WalletTransaction
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual UserWallet Wallet { get; set; } = null!;
}
