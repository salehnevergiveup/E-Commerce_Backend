using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface UserDetailsRepository
{
    public Task<UserDetail?> GetUserDetailById(int Id);  
    public Task<UserDetail> GetUserDetailByUserId(int Id);  

    public Task<int> CreateUserDetails(int id, UserDetail userDetails);  

    public Task<UserDetail> GetDetailByEmail(string email);

    public Task<bool> UpdateUserDetails(int id, UserDetail userDetails);
}
