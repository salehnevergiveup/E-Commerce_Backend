using System;
using System.ComponentModel.DataAnnotations;
using Mysqlx.Crud;

namespace PototoTrade.DTO.User;

public class CreateUserAccountDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", 
        ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role ID is required.")]
    public int RoleId { get; set; }
    public string Status { get; set; } = "Active";

}
