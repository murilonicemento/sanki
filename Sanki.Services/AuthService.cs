using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class AuthService : IAuthService
{
    private readonly SankiContext _sankiContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthService(SankiContext sankiContext, IPasswordService passwordService, IJwtService jwtService)
    {
        _sankiContext = sankiContext;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<LoginUserResponseDTO?> LoginAsync(LoginUserRequestDTO loginUserRequestDto)
    {
        var user = await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == loginUserRequestDto.Email);

        if (user is null) throw new InvalidOperationException("User is not registered.");

        var encryptedPassword = _passwordService.EncryptPassword(loginUserRequestDto.Password, user.Salt);
        var loggedUser = await _sankiContext.Users
            .Where(options => options.Email == loginUserRequestDto.Email && options.Password == encryptedPassword)
            .FirstOrDefaultAsync();

        return loggedUser is null ? null : _jwtService.GenerateJwt(loggedUser);
    }

    public async Task<LoginUserResponseDTO> GenerateNewAccessTokenAsync(TokenRequestDTO tokenRequestDto)
    {
        var principal = _jwtService.GetPrincipalFromJwt(tokenRequestDto.Token);

        if (principal is null) throw new SecurityTokenException("Invalid json web token.");

        var email = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == email);

        if (user is null || user.RefreshToken != tokenRequestDto.RefreshToken ||
            user.RefreshTokenExpiration <= DateTime.Now)
        {
            throw new SecurityTokenException("Invalid refresh token.");
        }

        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        user.RefreshToken = loginUserResponseDto.RefreshToken;
        user.RefreshTokenExpiration = loginUserResponseDto.RefreshTokenExpiration;

        await _sankiContext.SaveChangesAsync();

        return loginUserResponseDto;
    }
}