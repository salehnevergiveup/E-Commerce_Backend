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
        try
        {
            return await _context.Roles
                .Include(r => r.AdminPermissions)
                .Include(r => r.UserAccounts)
                .ToListAsync();
        }
        catch (Exception e)
        {
            return null;
        }

    }

    public async Task<Roles?> GetRoleAsync(int id)
    {
        try
        {
            return await _context.Roles
                        .Include(r => r.AdminPermissions)
                        .Include(r => r.UserAccounts)
                        .FirstOrDefaultAsync(r => r.Id == id);

        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<Roles> GetRoleByName(string roleName)
    {
        try
        {
            return await _context.Roles
                        .FirstOrDefaultAsync(r => r.RoleName == roleName);

        }
        catch (Exception e)
        {
            return null;
        }
    }


    public async Task<int> CreateRole(Roles role)
    {
        try
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role.Id;
        }
        catch (Exception e)
        {
            return 0;
        }
    }


    public async Task<bool> UpdateRole(Roles role)
    {
        try
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<bool> DeleteRole(Roles role)
    {
        try
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<Roles> GetDefaultRoleAsync()
    {
        try
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Default_Admin");
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
