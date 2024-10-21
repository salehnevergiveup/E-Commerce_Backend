using System;
using System.Collections.Generic;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Models.Product;

public partial class Products
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public bool MediaBoolean { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public string Status { get; set; } = null!;

    public int RefundGuaranteedDuration { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BuyerItems> BuyerItems { get; set; } = new List<BuyerItems>();

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();

    public virtual UserAccount User { get; set; } = null!;
}
