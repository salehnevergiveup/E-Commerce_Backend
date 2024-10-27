using System;
using PototoTrade.DTO.MediaDTO;

namespace PototoTrade.DTO.User;

public class CreateNewAccount
{
    public CreateUserAccountDTO UserAccount { get; set; } = new CreateUserAccountDTO();

    public CreateUserDetailDTO UserDetail { get; set; } = new CreateUserDetailDTO();

    public List<CreateMediaDTO>? MediaItems { get; set; } = null;
}

