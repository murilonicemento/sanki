using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class UserService : IUserService
{
    private readonly SankiContext _sankiContext;
    private readonly IJwtService _jwtService;

    public UserService(SankiContext sankiContext, IJwtService jwtService)
    {
        _sankiContext = sankiContext;
        _jwtService = jwtService;
    }

    public async Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserRequestDTO registerUserRequestDto)
    {
        var isUserAlreadyRegister = await IsUserAlreadyRegisterAsync(registerUserRequestDto);

        if (isUserAlreadyRegister) throw new InvalidOperationException("User already exist.");

        var user = new User
        {
            FirstName = registerUserRequestDto.FirstName,
            LastName = registerUserRequestDto.LastName,
            Email = registerUserRequestDto.Email,
        };

        var authResponseDto = _jwtService.GenerateJwt(user);

        var salt = new byte[128 / 8];
        var userSalt = GenerateSalt(salt);

        user.Password = EncryptPassword(registerUserRequestDto.Password, userSalt);
        user.Salt = userSalt;
        user.RefreshToken = authResponseDto.RefreshToken;
        user.RefreshTokenExpiration = authResponseDto.RefreshTokenExpiration;

        await _sankiContext.Users.AddAsync(user);
        await _sankiContext.SaveChangesAsync();

        return authResponseDto;
    }

    private async Task<bool> IsUserAlreadyRegisterAsync(RegisterUserRequestDTO registerUserRequestDto)
    {
        var user = await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == registerUserRequestDto.Email);

        return user is not null;
    }

    private byte[] GenerateSalt(byte[] salt)
    {
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        return salt;
    }

    private string EncryptPassword(string password, byte[] salt)
    {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8)
        );
    }
}