using System;
using PototoTrade.DTO.Role;
using PototoTrade.Models.Role;

namespace PototoTrade.Repository.Role;

public interface RoleRepository
{

    Task <List<Roles>> GetRolesAsync();

    Task <Roles?> GetRoleAsync(int Id); 

}
