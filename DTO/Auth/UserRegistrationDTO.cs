using System;
using System.ComponentModel.DataAnnotations;
using PototoTrade.DTO.User;

namespace PototoTrade.DTO.Auth;

public class UserRegistrationDTO
{    
    public UpdateViewUserAccountDTO UserAccount { get; set; } = new UpdateViewUserAccountDTO(); 

    public UpdateViewUserDetailDTO? UserDetails { get; set; } = new UpdateViewUserDetailDTO(); 
    
}
