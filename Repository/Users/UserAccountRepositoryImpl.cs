using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.Models;
using PototoTrade.Models.User;
using PototoTrade.Repository.Users;

namespace PototoTrade.Repository.User
{
    public class UserAccountRepositoryImpl : UserAccountRepository
    {
        private readonly DBC _context;

        public UserAccountRepositoryImpl(DBC context)
        {
            _context = context;
        }

        public async Task<List<UserAccount>> GetUsersList()
        {
            return await _context.UserAccounts.ToListAsync();
        }
        public async Task<UserAccount?> GetUserByUserNameAsync(string Username)
        {
            return await _context.UserAccounts.Include(u => u.Role)
                                        .FirstOrDefaultAsync(u => u.Username == Username);
        }

        public async Task<UserAccount?> GetUserByUserEmailAsync(string email)
        {
            return await _context.UserAccounts
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.UserDetails.Any(d => d.Email == email));
        }

         public async Task AddUserWithDetailsAsync(UserAccount user, UserDetail userDetails)
        {
            await _context.UserAccounts.AddAsync(user);
    
            await _context.SaveChangesAsync();

            userDetails.UserId = user.Id; 
           
            await _context.UserDetails.AddAsync(userDetails);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserPasswordAsync(UserAccount user)
        {
            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserAccount?> GetUserByIdAsync(int Id)
        {
            return await _context.UserAccounts.Include(u => u.Role)
                                        .FirstOrDefaultAsync(u => u.Id == Id);
        }
    }
}
