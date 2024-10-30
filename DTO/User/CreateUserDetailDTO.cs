using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.Models.User;

namespace PototoTrade.DTO.User;

public class CreateUserDetailDTO
{

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    public string? BillingAddress { get; set; }

    [Range(18, int.MaxValue, ErrorMessage = "Age must be 18 or older.")]
    public int? Age { get; set; }

    [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
    public string? Gender { get; set; }

    public static implicit operator CreateUserDetailDTO(UserDetail v)
    {
        throw new NotImplementedException();
    }
}
