using System;
using System.Collections.Generic;
using PototoTrade.Models.User;

namespace PototoTrade.Models.Product;

public partial class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public bool MediaBoolean { get; set; }

    public int Rating { get; set; }

    public string? ReviewComment { get; set; }

    public DateTime ReviewDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Products Product { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
