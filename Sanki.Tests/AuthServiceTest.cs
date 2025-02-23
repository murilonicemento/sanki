using AutoFixture;
using Microsoft.Extensions.Configuration;
using Moq;
using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class AuthServiceTest
{
    private readonly Fixture _fixture;
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IUserRepository _userRepository;
    private readonly Mock<IAuthRepository> _authRepositoryMock;

    public AuthServiceTest()
    {
        _fixture = new Fixture();
        var passwordService = new PasswordService();

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
        var authRepository = _authRepositoryMock.Object;

        _userService = new UserService(passwordService, _userRepository);
        _authService = new AuthService(passwordService, jwtService, _userRepository, authRepository);
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
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .With(user => user.Flashcards, new List<Flashcard>())
            .Create();
        var registerUserRequestDto = new RegisterUserRequestDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
        };

        _userRepositoryMock
            .Setup(temp => temp.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(null as User);
        _userRepositoryMock
            .Setup(temp => temp.RegisterUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _userService.RegisterAsync(registerUserRequestDto);

        var loginUserRequestDto = new LoginUserRequestDTO
        {
            Email = registerUserRequestDto.Email,
            Password = string.Empty
        };
        
        _userRepositoryMock
            .Setup(temp => temp.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _authRepositoryMock
            .Setup(temp => temp.GetLoggedUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as User);
        _authRepositoryMock
            .Setup(temp => temp.UpdateRefreshTokenAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _authService.LoginAsync(loginUserRequestDto);
        });
    }

    [Fact]
    public async Task LoginAsync_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .With(user => user.Flashcards, new List<Flashcard>())
            .Create();
        var registerUserRequestDto = new RegisterUserRequestDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
        };

        _userRepositoryMock
            .Setup(temp => temp.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(null as User);
        _userRepositoryMock
            .Setup(temp => temp.RegisterUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _userService.RegisterAsync(registerUserRequestDto);

        var loginUserRequestDto = new LoginUserRequestDTO
        {
            Email = registerUserRequestDto.Email,
            Password = registerUserRequestDto.Password
        };

        _userRepositoryMock
            .Setup(temp => temp.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _authRepositoryMock
            .Setup(temp => temp.GetLoggedUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(user);
        _authRepositoryMock
            .Setup(temp => temp.UpdateRefreshTokenAsync(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        await _authService.LoginAsync(loginUserRequestDto);

        var registeredUser = await _userRepository.GetUserByEmailAsync(loginUserRequestDto.Email);

        Assert.NotNull(registeredUser);
    }
}