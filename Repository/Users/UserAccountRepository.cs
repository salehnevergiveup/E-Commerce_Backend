


using PototoTrade.Models.Role;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task <List<UserAccount>> GetUsersList();
     
    Task<UserAccount?> GetUserByIdAsync(int Id); 
    Task <UserAccount?> GetUserByUserNameAsync(string userName);

    Task<UserAccount?>GetUserByUserEmailAsync(string email);

    Task AddUserWithDetailsAsync(UserAccount user, UserDetail userDetails);

    Task UpdateUserPasswordAsync(UserAccount user);


}

