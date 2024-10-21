using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.ShoppingCart;
using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Product;

public partial class PurchaseOrder
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CartId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime OrderCreatedAt { get; set; }

    public virtual ICollection<BuyerItems> BuyerItems { get; set; } = new List<BuyerItems>();

    public virtual ShoppingCarts Cart { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
