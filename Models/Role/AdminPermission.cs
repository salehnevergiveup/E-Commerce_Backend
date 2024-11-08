namespace PototoTrade.Models.Role;

public partial class AdminPermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanCreate { get; set; }

    public bool CanDelete { get; set; }

    public virtual Roles Role { get; set; } = null!;
}
