using System;
using System.Collections.Generic;

namespace PototoTrade.Models.BuyerItem;

public partial class BuyerItemDelivery
{
    public int Id { get; set; }

    public int BuyerItemId { get; set; }

    public string StageTypes { get; set; } = null!;

    public string StageDescription { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime StageDate { get; set; }

    public virtual BuyerItems BuyerItem { get; set; } = null!;
}
