using System;
using PototoTrade.Models.Role;

namespace PototoTrade.Repository.Role;

public interface RoleRepository
{
    Task <List<Roles>> GetUsersList();

}
