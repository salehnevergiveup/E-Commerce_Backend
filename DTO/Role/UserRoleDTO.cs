namespace PototoTrade.DTO.Role;

public record class UserRoleDTO
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
}
