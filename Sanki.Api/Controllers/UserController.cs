using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly SankiContext _sankiContext;
    private readonly IJwtService _jwtService;

    public UserController(SankiContext sankiContext, IJwtService jwtService)
    {
        _sankiContext = sankiContext;
        _jwtService = jwtService;
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterUserDTO registerUserDto)
    {
        if (!ModelState.IsValid)
        {
            var errorsList = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage)
                .ToList();
            var errorsString = string.Join(" | ", errorsList);

            return BadRequest(errorsString);
        }

        var authResponseDto = _jwtService.GenerateJwt(new User
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Email = registerUserDto.Email,
        });

        if (authResponseDto.Token is null
            || authResponseDto.RefreshToken is null
            || authResponseDto.RefreshTokenExpiration is null)
        {
            return Problem("An error occured. Contact the system admin.", statusCode: 500);
        }

        var salt = new byte[128 / 8];
        var userSalt = GenerateSalt(salt);

        var user = new User
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Email = registerUserDto.Email,
            Password = EncryptPassword(registerUserDto.Password, userSalt),
            Salt = userSalt,
            RefreshToken = authResponseDto.RefreshToken,
            RefreshTokenExpiration = authResponseDto.RefreshTokenExpiration.Value,
        };

        await _sankiContext.Users.AddAsync(user);
        await _sankiContext.SaveChangesAsync();

        return Ok(authResponseDto);
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
            numBytesRequested: 256 / 8));
    }
}