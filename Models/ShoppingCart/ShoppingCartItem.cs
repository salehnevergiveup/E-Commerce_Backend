using PototoTrade.Models.Product;
using System;
using System.Collections.Generic;

namespace PototoTrade.Models.ShoppingCart;

public partial class ShoppingCartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }

    public int ProductId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime AddedAt { get; set; }

    public virtual ShoppingCarts Cart { get; set; } = null!;

    public virtual Products Product { get; set; } = null!;
}
