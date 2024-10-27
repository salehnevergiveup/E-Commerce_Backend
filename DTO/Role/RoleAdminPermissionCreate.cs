using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.DTO.AdminPermission;

namespace PototoTrade.DTO.Role;

public class RoleAdminPermissionCreate
{
    [Required]
    public CreateRoleDTO Role { get; set; } = new CreateRoleDTO();
    public CreateAdminPermission Permission { get; set; } = null;
}
