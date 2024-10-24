using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class UpdatePasswordDTO
{

    [Required]
    public string CurrentPassword { get; set; } 
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", 
        ErrorMessage = "Password must contain at least one letter and one number.")]
    public string NewPassword { get; set; } 
}
