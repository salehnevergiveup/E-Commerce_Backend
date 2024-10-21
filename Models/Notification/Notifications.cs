using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Notification;

public partial class Notifications
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
