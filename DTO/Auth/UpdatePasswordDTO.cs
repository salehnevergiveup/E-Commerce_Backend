using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class UpdatePasswordDTO
{

    [Required]
    public string CurrentPassword { get; set; } // The current password for validation

    [Required]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
    public string NewPassword { get; set; } // The new password to be set
}
