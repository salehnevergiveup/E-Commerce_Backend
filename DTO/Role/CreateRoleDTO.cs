using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Role;

public class CreateRoleDTO
{
    [Required]
    public string RoleName { get; set; } = null!;

    [Required] 
    public string RoleType { get; set; } = null!;
    
    [Required]       public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
