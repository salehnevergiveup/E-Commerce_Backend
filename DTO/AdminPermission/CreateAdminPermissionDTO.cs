using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.AdminPermission;

public class CreateAdminPermission
{
    [Required]
    public bool CanView { get; set; } = true;  

    [Required]
    public bool CanEdit { get; set; } = false; 

    [Required]
    public bool CanCreate { get; set; } = false; 

    [Required]
    public bool CanDelete { get; set; }= false; 
}
