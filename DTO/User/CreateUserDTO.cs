using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.User;

public record class CreateUserDTO
{
    [Required]
    public int RoleId { get; set; }
    [Required]
    public string UserName { get; set; }
    public string UserCover { get; set; }
    public string Avatar { get; set; }
    [Required]
    public string Name { get; set; }
    [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
    [Required]
    public string Gender { get; set; }
    public int Age { get; set; }
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Required]
    public string Email { get; set; }
    [Required]
    public string Status { get; set; }
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; }
    public string BillingAddress { get; set; }
}
