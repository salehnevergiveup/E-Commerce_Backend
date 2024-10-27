using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.User;

public class UpdateViewUserDetailDTO
{
     [Required]
    public int Id {get; set;}
    
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? BillingAddress { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

}
