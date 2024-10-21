using PototoTrade.Models.Role.Role;
using System;
using System.Collections.Generic;

namespace PototoTrade.Models.Role;

public partial class Roles
{
    public int Id { get; set; }

    public string RoleName { get; set; } = null!;

    public string RoleType { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AdminPermission> AdminPermissions { get; set; } = new List<AdminPermission>();

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
