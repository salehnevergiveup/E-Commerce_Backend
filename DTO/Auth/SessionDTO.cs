using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class SessionDTO
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

}
