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
    }
}
