using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Product;

public partial class ProductCategory
{
    public int Id { get; set; }

    public string ProductCategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public double ChargeRate { get; set; }

    public double RebateRate { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();
}
