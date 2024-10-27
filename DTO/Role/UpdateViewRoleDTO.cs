using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.ShoppingCart;

public class UpdateViewRoleDTO
{
    [Required]
    public int Id {get; set;}
    public string RoleName { get; set; } = null!;

    public string RoleType { get; set; } = null!;
    
    public string? Description { get; set; }
    
}
