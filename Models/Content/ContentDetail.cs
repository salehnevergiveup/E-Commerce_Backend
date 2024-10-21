using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Content;

public partial class ContentDetail
{
    public int Id { get; set; }

    public int ContentId { get; set; }

    public bool MediaBoolean { get; set; }

    public string Description { get; set; } = null!;

    public string? Url { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Contents Content { get; set; } = null!;
}
