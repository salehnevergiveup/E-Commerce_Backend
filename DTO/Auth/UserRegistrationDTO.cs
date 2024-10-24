using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class UserRegistrationDTO
{    

    [Required]
    public string Name {get; set;}

    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    public string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", 
        ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    [Required]
    public string? BillingAddress { get; set; }

    [Required]
    [Range(18, int.MaxValue, ErrorMessage = "Age must be 18 or older.")]
    public int? Age { get; set; }

    [Required]
    [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
    public string? Gender { get; set; }
    
}
