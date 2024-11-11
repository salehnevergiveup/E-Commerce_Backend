using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.DTO.Role;

namespace PototoTrade.DTO.ShoppingCart;

public class UpdateViewRoleDTO
{
 public int Id {get; set;}
    public string RoleName { get; set; } = null!;

    public string RoleType { get; set; } = null!;
    
    public string? Description { get; set; }
    
}
