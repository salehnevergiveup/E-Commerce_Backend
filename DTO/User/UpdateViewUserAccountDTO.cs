using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace PototoTrade.DTO.User;

public class UpdateViewUserAccountDTO
{
    [Required]
    public int Id {get; set;}
    public string Name { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int RoleId { get; set; } = 0;
    
    public string Status { get; set; } = "Active";    

    public DateTime CreatedAt {get; set;}
    public DateTime? UpdatedAt {get; set;} 
}
