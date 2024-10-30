using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.DTO.User;

namespace PototoTrade.DTO.Auth;

public class UserRegistrationDTO
{    
    public CreateUserAccountDTO UserAccount { get; set; } = new CreateUserAccountDTO(); 

    public CreateUserDetailDTO? UserDetails { get; set; } = new CreateUserDetailDTO(); 
    
}
