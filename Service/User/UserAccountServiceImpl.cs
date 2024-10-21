using Microsoft.AspNetCore.Mvc;
using PototoTrade.Models;
using PototoTrade.Repository.Users;
using PototoTrade.Service.User;
namespace PototoTrade.Service.User
{
    public class UserAccountServiceImpl : IUserAccountService
    {
        private readonly UserAccountRepository _userAccountRepository;

        public UserAccountServiceImpl(UserAccountRepository userAccountRepository)
        {
            _userAccountRepository = userAccountRepository;
        }

        // Method to get all users
        public async Task<ActionResult<List<UserAccount>>> GetUserList()
        {
            try
            {
                var users = await _userAccountRepository.GetUsersList();
                return users;
            }
            catch (Exception ex)
            {
                // Log the error if any exception occurs

                throw;  // Re-throw the exception to ensure proper error handling
            }
        }

    }
}
