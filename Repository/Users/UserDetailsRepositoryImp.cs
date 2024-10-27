using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public class UserDetailsRepositoryImp : UserDetailsRepository
{
    private readonly DBC _context;

    public UserDetailsRepositoryImp(DBC context)
    {
        _context = context;
    }

    public async Task<int> CreateUserDetails(int id, UserDetail userDetail)
    {
        userDetail.UserId = id;
        _context.UserDetails.Add(userDetail);
        await _context.SaveChangesAsync();
        return userDetail.Id;
    }

    public async Task<UserDetail> GetDetailByEmail(string email)
    {
        return await _context.UserDetails
       .FirstOrDefaultAsync(d => d.Email == email);
    }

    public async Task<UserDetail?> GetUserDetailById(int Id)
    {
        return await _context.UserDetails.FirstOrDefaultAsync(d => d.Id == Id);

    }

    public async Task<UserDetail?> GetUserDetailByUserId(int Id)
    {
        return await _context.UserDetails.FirstOrDefaultAsync(d => d.UserId == Id);
    }

    public async Task<bool> UpdateUserDetails(int id, UserDetail userDetail)
    {
        _context.UserDetails.Update(userDetail);
        await _context.SaveChangesAsync();
        return true;

    }

}
