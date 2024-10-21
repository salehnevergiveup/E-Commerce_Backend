using System;
using System.Collections.Generic;

namespace PototoTrade.Models.User;

public partial class UserDetail
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? BillingAddress { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
