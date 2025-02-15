using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class AuthService : IAuthService
{
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;

    public AuthService(IPasswordService passwordService, IJwtService jwtService, IUserRepository userRepository, IAuthRepository authRepository)
    {
        _passwordService = passwordService;
        _jwtService = jwtService;
        _userRepository = userRepository;
        _authRepository = authRepository;
    }

    public async Task<LoginUserResponseDTO?> LoginAsync(LoginUserRequestDTO loginUserRequestDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(loginUserRequestDto.Email)
            ?? throw new InvalidOperationException("User is not registered.");
        var encryptedPassword = _passwordService.EncryptPassword(loginUserRequestDto.Password, user.Salt);
        var loggedUser = await _authRepository.GetLoggedUserAsync(loginUserRequestDto.Email, encryptedPassword)
            ?? throw new UnauthorizedAccessException("Password is incorrect.");
        var loginUserResponseDto = _jwtService.GenerateJwt(loggedUser);

        await _authRepository.UpdateRefreshTokenAsync(loggedUser, loginUserResponseDto.RefreshToken, loginUserResponseDto.RefreshTokenExpiration);

        return loginUserResponseDto;
    }

    public async Task<LoginUserResponseDTO> GenerateNewAccessTokenAsync(TokenRequestDTO tokenRequestDto)
    {
        var principal = _jwtService.GetPrincipalFromJwt(tokenRequestDto.Token)
            ?? throw new SecurityTokenException("Invalid json web token.");
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? throw new UnauthorizedAccessException("User is not authorized.");
        var user = await _authRepository.GetUserByEmailAsync(email);

        if (user is null || user.RefreshToken != tokenRequestDto.RefreshToken ||
            user.RefreshTokenExpiration <= DateTime.Now)
        {
            throw new SecurityTokenException("Invalid refresh token.");
        }

        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        await _authRepository.UpdateRefreshTokenAsync(user, loginUserResponseDto.RefreshToken, loginUserResponseDto.RefreshTokenExpiration);

        return loginUserResponseDto;
    }
}