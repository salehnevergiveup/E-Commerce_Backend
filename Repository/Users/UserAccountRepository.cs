


using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserAccountRepository
{
    Task<List<UserAccount>> GetUsersList();

}

