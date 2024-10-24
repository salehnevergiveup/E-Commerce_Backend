using System;
using System.Collections.Generic;
using PototoTrade.Models.Product;
using PototoTrade.Models.User;

namespace PototoTrade.Models.ShoppingCart;

public partial class ShoppingCarts
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();

    public virtual UserAccount User { get; set; } = null!;
}
