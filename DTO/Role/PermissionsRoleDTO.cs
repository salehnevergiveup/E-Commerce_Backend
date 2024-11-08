namespace PototoTrade.DTO.Role;

public record class PermissionsRoleDTO
{
    public int RoleId { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanCreate { get; set; }

    public bool CanDelete { get; set; }

}
