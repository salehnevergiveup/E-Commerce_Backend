

using PototoTrade.Models;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task<List<UserAccount>> GetUsersList();

}

