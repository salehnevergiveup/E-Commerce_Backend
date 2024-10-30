using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.User;

public class UserDto
{

    [Required]
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;

    [Range(18, int.MaxValue, ErrorMessage = "Age must be 18 or older.")]
    public int? Age { get; set; }

    [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
    public string Gender { get; set; } = string.Empty;

}