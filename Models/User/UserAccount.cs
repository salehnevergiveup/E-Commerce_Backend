using System;
using System.Collections.Generic;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.Notification;
using PototoTrade.Models.Product;
using PototoTrade.Models.Role;
using PototoTrade.Models.ShoppingCart;

namespace PototoTrade.Models.User;

public partial class UserAccount
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Chatroom { get; set; }

    public virtual ICollection<BuyerItems> BuyerItems { get; set; } = new List<BuyerItems>();

    public virtual ICollection<Notifications> Notifications { get; set; } = new List<Notifications>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();

    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    public virtual Roles Role { get; set; } = null!;

    public virtual ShoppingCarts? ShoppingCart { get; set; }

    public virtual ICollection<UserActivitiesLog> UserActivitiesLogs { get; set; } = new List<UserActivitiesLog>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<UserReport> UserReportReportedUsers { get; set; } = new List<UserReport>();

    public virtual ICollection<UserReport> UserReportReporterUsers { get; set; } = new List<UserReport>();

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public virtual UserWallet? UserWallet { get; set; }
}
