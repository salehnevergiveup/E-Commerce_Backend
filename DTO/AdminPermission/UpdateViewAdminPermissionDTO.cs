using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.AdminPermission;

public class UpdateViewAdminPermissionDTO
{
    public int Id {get; set;} 
    [Required]
    public int RoleId {get; set;} 
    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanCreate { get; set; }

    public bool CanDelete { get; set; }
}
