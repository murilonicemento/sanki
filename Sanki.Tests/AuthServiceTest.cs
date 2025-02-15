using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class AuthServiceTest
{
    private readonly Fixture _fixture;
    private readonly PasswordService _passwordService;
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IUserRepository _userRepository;
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly IAuthRepository _authRepository;

    public AuthServiceTest()
    {
        _fixture = new Fixture();
        _passwordService = new PasswordService();

        var initialData = new Dictionary<string, string>
        {
            { "Jwt:Key", "12345678901234567890123456789012" },
            { "Jwt:EXPIRATION_MINUTES", "720" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();
        var jwtService = new JwtService(configuration);

        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepository = _userRepositoryMock.Object;
        _authRepositoryMock = new Mock<IAuthRepository>();
        _authRepository = _authRepositoryMock.Object;

        _userService = new UserService(_passwordService, _userRepository);
        _authService = new AuthService(_passwordService, jwtService, _userRepository, _authRepository);
    }

    [Fact]
    public async Task LoginAsync_UserNotRegistered_ShouldBeInvalidOperationException()
    {
        var loginUserRequestDto = _fixture.Build<LoginUserRequestDTO>().Create();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _authService.LoginAsync(loginUserRequestDto);
        });
    }

    [Fact]
    public async Task LoginAsync_IncorrectPassword_ShouldReturnNull()
    {
        var registerUserRequestDto = _fixture.Build<RegisterUserRequestDTO>().Create();

        await _userService.RegisterAsync(registerUserRequestDto);

        var loginUserRequestDto = new LoginUserRequestDTO
        {
            Email = registerUserRequestDto.Email,
            Password = string.Empty
        };

        var loginUserResponseDto = await _authService.LoginAsync(loginUserRequestDto);

        Assert.Null(loginUserResponseDto);
    }

    [Fact]
    public async Task LoginAsync_ShouldBeValid()
    {
        var registerUserRequestDto = _fixture.Build<RegisterUserRequestDTO>().Create();

        await _userService.RegisterAsync(registerUserRequestDto);

        var loginUserRequestDto = new LoginUserRequestDTO
        {
            Email = registerUserRequestDto.Email,
            Password = registerUserRequestDto.Password
        };

        await _authService.LoginAsync(loginUserRequestDto);

        var user = await _userRepository.GetUserByEmailAsync(loginUserRequestDto.Email);
        var password = _passwordService.EncryptPassword(registerUserRequestDto.Password, user.Salt);

        Assert.True(user.Password == password);
    }
}