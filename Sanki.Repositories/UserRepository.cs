using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;

namespace Sanki.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SankiContext _sankiContext;

    public UserRepository(SankiContext sankiContext)
    {
        _sankiContext = sankiContext;
    }

    public async Task RegisterUserAsync(User user)
    {
        await _sankiContext.Users.AddAsync(user);
        await _sankiContext.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == email);
    }
}

