using PototoTrade.Models.Role;

namespace PototoTrade.DTO.Role;

public record class UpdateRoleDTO
{
   public string? Description { get; set; }
   public PermissionsRoleDTO Permissions {get; set;} =  new PermissionsRoleDTO();  
}
