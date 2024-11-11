using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.DTO.Auth;
using PototoTrade.Enums;
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
            return await _context.UserAccounts
                 .Include(u => u.Role)
                .Include(u => u.UserDetails).ToListAsync();
        }

        public async Task<int> CreateNewUser(UserAccount newUser)
        {

            await _context.UserAccounts.AddAsync(newUser);

            await _context.SaveChangesAsync();

            return newUser.Id;
        }

        public async Task UpdateUserPasswordAsync(UserAccount user)
        {
             _context.UserAccounts.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task<UserAccount?> GetUserByIdAsync(int Id)
        {
            return await _context.UserAccounts.Include(u => u.Role)
                                               .Include(u => u.Products)
                                               .Include(u => u.UserDetails)
                                               .Include(u => u.ProductReviews)
                                               .FirstOrDefaultAsync(u => u.Id == Id);
        }

        public async Task<UserAccount?> GetUserByUserNameOrEmailAsync(string input)
        {
            return await _context.UserAccounts
                .Include(u => u.Role)
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Username == input || u.UserDetails.Any(d => d.Email == input));
        }

        public async Task<UserAccount> GetUserByPhoneNumber(string phoneNumber)
        {
            return await _context.UserAccounts
                         .Include(u => u.UserDetails)
                         .FirstOrDefaultAsync(u => u.UserDetails.Any(d => d.PhoneNumber == phoneNumber));
        }
        public async Task<bool> UpdateUserAsync(UserAccount userAccount)
        {
            _context.UserAccounts.Update(userAccount);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.Id == id);

            _context.UserAccounts.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserAccount>> GetAdminsByRoleIdAsync(int roleId)
        {
            try
            {
                return await _context.UserAccounts.Where(u => u.RoleId == roleId && u.Role.RoleName == UserRolesEnum.Admin.ToString()).ToListAsync();
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<bool> UpdateUserAccountsAsync(List<UserAccount> userAccounts)
        {
            try
            {
                _context.UpdateRange(userAccounts);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

    }
}
