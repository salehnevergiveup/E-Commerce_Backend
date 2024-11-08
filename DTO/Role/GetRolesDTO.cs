using PototoTrade.Models.User;
using PototoTrade.Models.Role;

namespace PototoTrade.DTO.Role;

public record class GetRolesDTO
{
    public int Id {get; set;}
    public string RoleName { get; set; } = null!;

    public string RoleType { get; set; } = null!;
    
    public string? Description { get; set; }

    public DateTime CreatedAt {get; set;} 

    public List<UserRoleDTO> Users {get; set;}=  new  List<UserRoleDTO>();

   public PermissionsRoleDTO Permissions {get; set;} =  new PermissionsRoleDTO();  

}
