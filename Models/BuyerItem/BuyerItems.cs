using System;
using System.Collections.Generic;
using PototoTrade.Models.Product;
using PototoTrade.Models.User;

namespace PototoTrade.Models.BuyerItem;

public partial class BuyerItems
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int BuyerId { get; set; }

    public DateOnly? ArrivedDate { get; set; }

    public DateOnly? ValidRefundDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserAccount Buyer { get; set; } = null!;

    public virtual ICollection<BuyerItemDelivery> BuyerItemDeliveries { get; set; } = new List<BuyerItemDelivery>();

    public virtual PurchaseOrder Order { get; set; } = null!;

    public virtual Products Product { get; set; } = null!;
}
