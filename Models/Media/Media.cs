using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Media;

public partial class Media 
{
    public int Id { get; set; }

    public string SourceType { get; set; } = null!;

    public int SourceId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
