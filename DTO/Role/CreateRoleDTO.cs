using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.Models.Role;

namespace PototoTrade.DTO.Role;

public class CreateRoleDTO
{
    [Required]
    public string RoleName { get; set; } = null!;
    
    [Required] public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public PermissionsRoleDTO Permission {get; set;} = new PermissionsRoleDTO();

}
