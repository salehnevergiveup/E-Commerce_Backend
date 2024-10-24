


using PototoTrade.Models.Role;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task <List<UserAccount>> GetUsersList();
     
    Task<UserAccount?> GetUserByIdAsync(int Id); 

    Task<UserAccount?>GetUserByUserNameOrEmailAsync(string input);

    Task AddUserWithDetailsAsync(UserAccount user, UserDetail userDetails);

    Task UpdateUserPasswordAsync(UserAccount user);
}

