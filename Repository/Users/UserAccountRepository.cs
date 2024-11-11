


using PototoTrade.DTO.Auth;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task<List<UserAccount>> GetUsersList();

    Task<UserAccount?> GetUserByIdAsync(int id);

    Task<bool> UpdateUserAsync(UserAccount userAccount);

    Task<bool> DeleteUserAsync(int id);

    Task<UserAccount?> GetUserByUserNameOrEmailAsync(string input);

    Task<int> CreateNewUser(UserAccount newUserDto);

    Task UpdateUserPasswordAsync(UserAccount user);

    Task<List<UserAccount>> GetAdminsByRoleIdAsync(int roleId);

    Task <bool> UpdateUserAccountsAsync(List<UserAccount> userAccounts);

    Task<UserAccount?> GetUserByPhoneNumber(string phoneNumber); 

}

