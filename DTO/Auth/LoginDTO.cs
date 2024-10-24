using System;
using System.ComponentModel.DataAnnotations;

namespace PototoTrade.DTO.Auth;

public class LoginDTO
{
    [Required(ErrorMessage = "Insert your email or username plasee")]
    public string EmailOrUsername { get; set; }

    [Required(ErrorMessage = "Password is Requred")]
    public string Password { get; set; }

    public bool RememberMe { get; set; }

}
