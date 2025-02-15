using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class UserService : IUserService
{
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;

    public UserService(IPasswordService passwordService, IUserRepository userRepository)
    {
        _passwordService = passwordService;
        _userRepository = userRepository;
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

        var salt = new byte[128 / 8];
        var userSalt = _passwordService.GenerateSalt(salt);

        user.Password = _passwordService.EncryptPassword(registerUserRequestDto.Password, userSalt);
        user.Salt = userSalt;

        await _userRepository.RegisterUserAsync(user);

        return new RegisterUserResponseDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
        };
    }

    private async Task<bool> IsUserAlreadyRegisterAsync(RegisterUserRequestDTO registerUserRequestDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(registerUserRequestDto.Email);

        return user is not null;
    }
}