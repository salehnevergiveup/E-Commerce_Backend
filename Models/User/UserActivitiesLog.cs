using System;
using System.Collections.Generic;

namespace PototoTrade.Models;

public partial class UserActivitiesLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ActivityName { get; set; } = null!;

    public string ActivityInfo { get; set; } = null!;

    public DateTime ActivityDate { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
