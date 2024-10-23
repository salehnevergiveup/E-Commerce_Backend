using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class UserRegistrationDTO
{
    [Required]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
    public string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; }

    [Required]
    public string? PhoneNumber { get; set; }

    [Required]
    public string? BillingAddress { get; set; }

    [Required]
    public int? Age { get; set; }

    [Required]
    public string? Gender { get; set; }
    
}
