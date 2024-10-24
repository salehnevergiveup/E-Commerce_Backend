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
    public async Task<List<Roles>> GetUsersList()
    {
         return await _context.Roles.ToListAsync();
    }
}
