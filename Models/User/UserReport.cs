using System;
using System.Collections.Generic;

namespace PototoTrade.Models;

public partial class UserReport
{
    public int Id { get; set; }

    public int ReportedUserId { get; set; }

    public int ReporterUserId { get; set; }

    public string ReportType { get; set; } = null!;

    public string ReportComment { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime ReportDate { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual UserAccount ReportedUser { get; set; } = null!;

    public virtual UserAccount ReporterUser { get; set; } = null!;
}
