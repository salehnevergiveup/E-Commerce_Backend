using System;
using PototoTrade.DTO.Auth;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public interface SessionRepository
{
    Task <UserSession?> GetSessionByUser(int Id);
    Task  SetSession(SessionDTO session);
    Task UpdateSessionAsync(UserSession session);

    Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken);

}
