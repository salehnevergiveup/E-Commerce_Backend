using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class UpdatePasswordDTO
{

    [Required]
    public string CurrentPassword { get; set; } 
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; } 
}
