


using PototoTrade.DTO.Auth;
using PototoTrade.DTO.User;
using PototoTrade.Models.Role;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task<List<UserAccount>> GetUsersList();

    Task<UserAccount?> GetUserByIdAsync(int id);

    Task<bool> UpdateUserAsync(int id, UserAccount userAccount);

    Task<bool> DeleteUserAsync(int id);

    Task<UserAccount?> GetUserByUserNameOrEmailAsync(string input);

    Task<int> CreateNewUser(UserAccount newUserDto);

    Task UpdateUserPasswordAsync(int id, UpdatePasswordDTO passwords);

    Task<List<UserAccount>> GetAdminsByRoleIdAsync(int roleId);

    Task <bool> UpdateUserAccountsAsync(List<UserAccount> userAccounts);

}

