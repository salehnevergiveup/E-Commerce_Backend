using System;
using PototoTrade.DTO.Role;
using PototoTrade.Models.Role;

namespace PototoTrade.Repository.Role;

public interface RoleRepository
{
    Task<List<Roles>> GetRolesAsync();
    Task<Roles?> GetRoleAsync(int Id);
    Task<int> CreateRole(Roles role);
    Task<bool> DeleteRole(Roles role);
    Task<Roles> GetDefaultRoleAsync();
    Task<bool> UpdateRole(Roles role); 
    Task<Roles> GetRoleByName(string roleName);
}
