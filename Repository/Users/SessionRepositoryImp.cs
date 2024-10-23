using System;
using Microsoft.EntityFrameworkCore;
using PototoTrade.Data;
using PototoTrade.DTO.Auth;
using PototoTrade.Models.User;

namespace PototoTrade.Repository.Users;

public class SessionRepositoryImp : SessionRepository
{
    private readonly DBC _context;

    public SessionRepositoryImp(DBC context)
    {
        _context = context;
    }

    public Task<UserSession?> GetSessionByUser(int Id)
    {
        throw new NotImplementedException();
    }


    public async Task SetSession(SessionDTO session)
    {
        var userSession = new UserSession
        {
            UserId = session.UserId,
            AccessToken = session.AccessToken,
            RefreshToken = session.RefreshToken,
            ExpiresAt = session.ExpiresAt,
            IsRevoked = session.IsRevoked,
            CreatedAt = DateTime.UtcNow
        };

        await _context.UserSessions.AddAsync(userSession);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(UserSession session)
    {
        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task<UserSession?> GetSessionByRefreshTokenAsync(string refreshToken)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsRevoked && s.ExpiresAt > DateTime.UtcNow);
    }

}
