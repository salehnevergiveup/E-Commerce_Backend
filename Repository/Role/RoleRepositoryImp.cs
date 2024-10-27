using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.Role;

namespace PototoTrade.Repository.Role;

public class RoleRepositoryImp : RoleRepository
{
    private readonly DBC _context;

    public RoleRepositoryImp(DBC context)
    {
        _context = context;
    }
    public async Task<List<Roles>> GetRolesAsync()
    {
        return await _context.Roles
            .Include(r => r.AdminPermissions)
            .ToListAsync();
    }

    public async Task<Roles?> GetRoleAsync(int Id)
    {

        return await _context.Roles.Include(r => r.AdminPermissions).FirstOrDefaultAsync(r => r.Id == Id);

        // return  new UpdateViewRoleAdminPermission
        // {
        //     Role = new UpdateViewRoleDTO
        //     {
        //         Id = role.Id,
        //         RoleName = role.RoleName,
        //         RoleType = role.RoleType,
        //         Description = role.Description,
        //     },
        //     Permission = role.AdminPermissions.FirstOrDefault() != null 
        //         ? new UpdateViewAdminPermission
        //         {
        //             Id = role.AdminPermissions.First().Id,
        //             CanCreate = role.AdminPermissions.First().CanCreate,  
        //             CanDelete = role.AdminPermissions.First().CanDelete,  
        //             CanEdit = role.AdminPermissions.First().CanEdit,  
        //             CanView = role.AdminPermissions.First().CanView,  
        //         } 
        //         : null
        //     };

    }
}
