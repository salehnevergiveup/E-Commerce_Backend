using System;
using PototoTrade.DTO.MediaDTO;

namespace PototoTrade.DTO.User;

public class UpdateUserAccountDTO
{
    public UpdateViewUserAccountDTO UserAccount { get; set; } = new UpdateViewUserAccountDTO();

    public UpdateViewUserDetailDTO UserDetail { get; set; } = new UpdateViewUserDetailDTO();

    public List<UpdateViewMediaDTO>? MediaItems { get; set; } = null;

}
