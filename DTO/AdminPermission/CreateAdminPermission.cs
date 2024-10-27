using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.AdminPermission;

public class CreateAdminPermission
{
    [Required]
    public bool CanView { get; set; }

    [Required]
    public bool CanEdit { get; set; }

    [Required]
    public bool CanCreate { get; set; }

    [Required]
    public bool CanDelete { get; set; }
}
