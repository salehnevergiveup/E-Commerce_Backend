using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.User;

public record class UpdateUserDTO
{
    public int RoleId { get; set; }
    public string UserName { get; set; }
    public string UserCover { get; set; }
    public string Avatar { get; set; }
    public string Name { get; set; }
    [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
    public string Gender { get; set; }
    public int Age { get; set; }
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    public string Status { get; set; }
    [Phone(ErrorMessage = "Invalid phone number.")]
    public string PhoneNumber { get; set; }
    public string BillingAddress { get; set; }

}
