using Sanki.Entities;

namespace Sanki.Repositories.Contracts;

public interface IUserRepository
{
    public Task RegisterUserAsync(User user);
    public Task<User?> GetUserByEmailAsync(string email);
}

