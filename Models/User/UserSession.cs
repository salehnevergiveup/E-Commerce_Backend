using System;
using System.Collections.Generic;

namespace PototoTrade.Models.User;

public partial class UserSession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public DateTime? RevokedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
