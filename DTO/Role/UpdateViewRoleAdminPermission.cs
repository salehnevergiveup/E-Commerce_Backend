using PototoTrade.DTO.AdminPermission;
using PototoTrade.DTO.ShoppingCart;

namespace PototoTrade.DTO.Role;

public class UpdateViewRoleAdminPermission
{
    public UpdateViewRoleDTO Role { get; set; } = new UpdateViewRoleDTO();
    public UpdateViewAdminPermission Permission { get; set; } = null;

}
