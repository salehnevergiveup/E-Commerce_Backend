using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Content;

public partial class Contents
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string UniqueCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();
}
