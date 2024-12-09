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

//todo: add dto for userwallet, modify controller so that api functions uses dto and not optional query