using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Models.BuyerItem;
using PototoTrade.Models.Content;
using PototoTrade.Models.Media;
using PototoTrade.Models.Notification;
using PototoTrade.Models.Product;
using PototoTrade.Models.Role;
using PototoTrade.Models.ShoppingCart;
using PototoTrade.Models.User;


namespace PototoTrade.Data;

public partial class DBC : DbContext
{
    public DBC()
    {
    }

    public DBC(DbContextOptions<DBC> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminPermission> AdminPermissions { get; set; }

    public virtual DbSet<BuyerItems> BuyerItems { get; set; }

    public virtual DbSet<BuyerItemDelivery> BuyerItemDeliveries { get; set; }

    public virtual DbSet<Contents> Contents { get; set; }

    public virtual DbSet<ContentDetail> ContentDetails { get; set; }

    public virtual DbSet<Media> Media { get; set; }

    public virtual DbSet<Notifications> Notifications { get; set; }

    public virtual DbSet<UserNotification> UserNotification {get; set;}
    public virtual DbSet<Products> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<ShoppingCarts> ShoppingCarts { get; set; }

    public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserActivitiesLog> UserActivitiesLogs { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    public virtual DbSet<UserReport> UserReports { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<UserWallet> UserWallets { get; set; }
    public virtual DbSet<RefundRequest> RefundRequests { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<OnHoldingPaymentHistory> OnHoldingPaymentHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;database=e_commerce_second_hand;user=root;password=abc123", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.37-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdminPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("admin_permissions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RoleId, "role_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CanCreate).HasColumnName("can_create");
            entity.Property(e => e.CanDelete).HasColumnName("can_delete");
            entity.Property(e => e.CanEdit).HasColumnName("can_edit");
            entity.Property(e => e.CanView).HasColumnName("can_view");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.AdminPermissions)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("admin_permissions_ibfk_1");
        });

        modelBuilder.Entity<BuyerItems>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("buyer_item")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.BuyerId, "buyer_id");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.HasIndex(e => e.ProductId, "product_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArrivedDate).HasColumnName("arrived_date");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.ValidRefundDate).HasColumnName("valid_refund_date");

            entity.HasOne(d => d.Buyer).WithMany(p => p.BuyerItems)
                .HasForeignKey(d => d.BuyerId)
                .HasConstraintName("buyer_item_ibfk_3");

            entity.HasOne(d => d.Order).WithMany(p => p.BuyerItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("buyer_item_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.BuyerItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("buyer_item_ibfk_2");
        });

        modelBuilder.Entity<BuyerItemDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("buyer_item_delivery")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.BuyerItemId, "buyer_item_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyerItemId).HasColumnName("buyer_item_id");
            entity.Property(e => e.StageDate)
                .HasColumnType("timestamp")
                .HasColumnName("stage_date");
            entity.Property(e => e.StageDescription)
                .HasMaxLength(255)
                .HasColumnName("stage_description");
            entity.Property(e => e.StageTypes)
                .HasMaxLength(50)
                .HasColumnName("stage_types");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.BuyerItem).WithMany(p => p.BuyerItemDeliveries)
                .HasForeignKey(d => d.BuyerItemId)
                .HasConstraintName("buyer_item_delivery_ibfk_1");
        });

        modelBuilder.Entity<Contents>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("content")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UniqueCode, "unique_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UniqueCode)
                .HasMaxLength(50)
                .HasColumnName("unique_code");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<ContentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("content_details")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ContentId, "content_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.MediaBoolean).HasColumnName("media_boolean");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.Content).WithMany(p => p.ContentDetails)
                .HasForeignKey(d => d.ContentId)
                .HasConstraintName("content_details_ibfk_1");
        });

        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("media")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .HasColumnName("media_url");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceType)
                .HasMaxLength(10)
                .HasColumnName("source_type");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Notifications>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notification")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.SenderId, "sender_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReceiverId)
                .HasColumnType("int")
                .HasColumnName("ReceiverId");
             entity.Property(e => e.ReceiverUsername)
                .HasColumnType("longtext")
                .HasColumnName("ReceiverUsername");
            entity.Property(e => e.Type)
                .HasColumnType("text")
                .HasColumnName("Type");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.MessageText)
                .HasColumnType("text")
                .HasColumnName("message_text");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderUsername)
                .HasColumnType("longtext")
                .HasColumnName("SenderUsername");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("notification_ibfk_1");
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.UserNotificationId).HasName("PRIMARY");

            entity.ToTable("user_notification")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.UserNotificationId)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.UserUsername)
                .HasColumnType("longtext")
                .HasColumnName("UserUsername");

            entity.Property(e => e.NotificationId)
                .HasColumnName("notification_id")
                .IsRequired();

            entity.Property(e => e.IsRead)
                .HasColumnName("is_read")
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.ReceivedAt)
                .HasColumnName("received_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserNotifications)
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("user_notification_ibfk_1");

            entity.HasOne(e => e.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(e => e.NotificationId)
                .HasConstraintName("user_notification_ibfk_2");
        });

        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("products")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CategoryId, "category_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.MediaBoolean).HasColumnName("media_boolean");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.RefundGuaranteedDuration).HasColumnName("refund_guaranteed_duration");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.Products)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("products_ibfk_1");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("product_category")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ProductCategoryName, "product_category_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChargeRate).HasColumnName("charge_rate");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ProductCategoryName)
                .HasMaxLength(100)
                .HasColumnName("product_category_name");
            entity.Property(e => e.RebateRate).HasColumnName("rebate_rate");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("product_review")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ProductId, "product_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MediaBoolean).HasColumnName("media_boolean");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewComment)
                .HasColumnType("text")
                .HasColumnName("review_comment");
            entity.Property(e => e.ReviewDate)
                .HasColumnType("timestamp")
                .HasColumnName("review_date");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_review_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("product_review_ibfk_2");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("purchase_order")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CartId, "cart_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.OrderCreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("order_created_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cart).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("purchase_order_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.PurchaseOrders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("purchase_order_ibfk_1");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("roles")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RoleName, "role_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
            entity.Property(e => e.RoleType)
                .HasMaxLength(50)
                .HasColumnName("role_type");
        });

        modelBuilder.Entity<ShoppingCarts>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("shopping_cart")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.ShoppingCart)
                .HasForeignKey<ShoppingCarts>(d => d.UserId)
                .HasConstraintName("shopping_cart_ibfk_1");
        });

        modelBuilder.Entity<ShoppingCartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("shopping_cart_items")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CartId, "cart_id");

            entity.HasIndex(e => e.ProductId, "product_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedAt)
                .HasColumnType("timestamp")
                .HasColumnName("added_at");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Cart).WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("shopping_cart_items_ibfk_1");

            entity.HasOne(d => d.Product).WithMany(p => p.ShoppingCartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shopping_cart_items_ibfk_2");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_accounts")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RoleId, "role_id");

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_accounts_ibfk_1");
        });

        modelBuilder.Entity<UserActivitiesLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_activities_log")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActivityDate)
                .HasColumnType("timestamp")
                .HasColumnName("activity_date");
            entity.Property(e => e.ActivityInfo)
                .HasColumnType("text")
                .HasColumnName("activity_info");
            entity.Property(e => e.ActivityName)
                .HasMaxLength(100)
                .HasColumnName("activity_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivitiesLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_activities_log_ibfk_1");
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_details")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.BillingAddress)
                .HasColumnType("text")
                .HasColumnName("billing_address");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(15)
                .HasColumnName("gender");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserDetails)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_details_ibfk_1");
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_report")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ReportedUserId, "reported_user_id");

            entity.HasIndex(e => e.ReporterUserId, "reporter_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportComment)
                .HasColumnType("text")
                .HasColumnName("report_comment");
            entity.Property(e => e.ReportDate)
                .HasColumnType("timestamp")
                .HasColumnName("report_date");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .HasColumnName("report_type");
            entity.Property(e => e.ReportedUserId).HasColumnName("reported_user_id");
            entity.Property(e => e.ReporterUserId).HasColumnName("reporter_user_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ReportedUser).WithMany(p => p.UserReportReportedUsers)
                .HasForeignKey(d => d.ReportedUserId)
                .HasConstraintName("user_report_ibfk_1");

            entity.HasOne(d => d.ReporterUser).WithMany(p => p.UserReportReporterUsers)
                .HasForeignKey(d => d.ReporterUserId)
                .HasConstraintName("user_report_ibfk_2");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_session")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessToken)
                .HasMaxLength(500)
                .HasColumnName("access_token");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("timestamp")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500)
                .HasColumnName("refresh_token");
            entity.Property(e => e.RevokedAt)
                .HasColumnType("timestamp")
                .HasColumnName("revoked_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_session_ibfk_1");
        });

        modelBuilder.Entity<UserWallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_wallet")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "user_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvailableBalance)
                .HasPrecision(10, 2)
                .HasColumnName("available_balance");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasColumnName("currency");
            entity.Property(e => e.OnHoldBalance)
                .HasPrecision(10, 2)
                .HasColumnName("on_hold_balance");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.UserWallet)
                .HasForeignKey<UserWallet>(d => d.UserId)
                .HasConstraintName("user_wallet_ibfk_1");
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.HasKey(e => e.RefundRequestId).HasName("PRIMARY");

            entity.ToTable("refund_requests")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.RefundRequestId)
                .HasColumnName("RefundRequestId")
                .IsRequired();

            entity.Property(e => e.BuyerId)
                .HasColumnName("BuyerId")
                .IsRequired();

            entity.Property(e => e.SellerId)
                .HasColumnName("SellerId")
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("Amount")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnType("varchar(50)")
                .HasColumnName("Status")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("CreatedAt");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .HasColumnName("UpdatedAt");

            // Foreign key constraints (optional but recommended for clarity)
            entity.HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .HasConstraintName("refund_requests_ibfk_1");

            entity.HasOne<UserAccount>()
                .WithMany()
                .HasForeignKey(e => e.SellerId)
                .HasConstraintName("refund_requests_ibfk_2");
        });


        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("wallet_transaction")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.WalletId, "wallet_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(100)
                .HasColumnName("transaction_type");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("wallet_transaction_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
