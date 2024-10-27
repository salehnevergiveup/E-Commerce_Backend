using System;
using PototoTrade.DTO.MediaDTO;
using PototoTrade.DTO.Role;

namespace PototoTrade.DTO.User;

public class ViewUserAccount
{
    public UpdateViewUserAccountDTO UserAccount { get; set; } = new UpdateViewUserAccountDTO(); 

    public UpdateViewUserDetailDTO? UserDetails { get; set; } = new UpdateViewUserDetailDTO(); 

    public List<UpdateViewRoleAdminPermission>? AvailableRoles { get; set; } = new List<UpdateViewRoleAdminPermission>(); 

    public ICollection<UpdateViewMediaDTO>? MediaItems { get; set; } = new List<UpdateViewMediaDTO>();
}
