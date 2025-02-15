using Sanki.Entities;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Repositories.Contracts;

public interface IAuthRepository
{
    public Task<User?> GetLoggedUserAsync(string email, string password);
    public Task UpdateRefreshTokenAsync(User loggedUser, string refreshToken, DateTime refreshTokenExpiration);
}
