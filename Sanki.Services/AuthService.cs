using Microsoft.EntityFrameworkCore;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class AuthService : IAuthService
{
    private readonly SankiContext _sankiContext;
    private readonly IPasswordService _passwordService;

    public AuthService(SankiContext sankiContext, IPasswordService passwordService)
    {
        _sankiContext = sankiContext;
        _passwordService = passwordService;
    }

    public async Task<LoginUserResponseDTO?> LoginAsync(LoginUserRequestDTO loginUserRequestDto)
    {
        var user = await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == loginUserRequestDto.Email);

        if (user is null) throw new InvalidOperationException("User is not registered.");

        var encryptedPassword = _passwordService.EncryptPassword(loginUserRequestDto.Password, user.Salt);
        var loggedUser = await _sankiContext.Users
            .Where(options => options.Email == loginUserRequestDto.Email && options.Password == encryptedPassword)
            .FirstOrDefaultAsync();

        if (loggedUser is null) return null;

        return new LoginUserResponseDTO
        {
            FirstName = loggedUser.FirstName,
            LastName = loggedUser.LastName,
            Email = loggedUser.Email,
            Token = loginUserRequestDto.Token,
            RefreshToken = loggedUser.RefreshToken,
            RefreshTokenExpiration = loggedUser.RefreshTokenExpiration
        };
    }
}