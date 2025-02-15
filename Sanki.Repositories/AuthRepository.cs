using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly SankiContext _sankiContext;

    public AuthRepository(SankiContext sankiContext)
    {
        _sankiContext = sankiContext;
    }

    public async Task<User?> GetLoggedUserAsync(string email, string password)
    {
        return await _sankiContext.Users
            .Where(user => user.Email == email && user.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateRefreshTokenAsync(User loggedUser, string refreshToken, DateTime refreshTokenExpiration)
    {
        loggedUser.RefreshToken = refreshToken;
        loggedUser.RefreshTokenExpiration = refreshTokenExpiration;

        await _sankiContext.SaveChangesAsync();
    }

}

