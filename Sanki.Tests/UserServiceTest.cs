using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sanki.Persistence;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class UserServiceTest
{
    private readonly Fixture _fixture;
    private readonly UserService _userService;
    private readonly SankiContext _sankiContext;

    public UserServiceTest()
    {
        _fixture = new Fixture();

        var options = new DbContextOptionsBuilder<SankiContext>()
            .UseInMemoryDatabase(databaseName: "SankiTestDatabase")
            .Options;
        _sankiContext = new SankiContext(options);

        _sankiContext.Database.EnsureCreated();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "Jwt:Key", "12345678901234567890123456789012" },
            { "Jwt:EXPIRATION_MINUTES", "720" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        var jwtService = new JwtService(configuration);

        _userService = new UserService(_sankiContext, jwtService);
    }

    #region Register

    [Fact]
    public async Task Register_PersonAlreadyExist_ShouldBeInvalidOperationException()
    {
        var registerUserDto = _fixture.Build<RegisterUserDTO>().Create();

        await _userService.Register(registerUserDto);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _userService.Register(registerUserDto);
        });
    }

    [Fact]
    public async Task Register_ShouldReturnValidAuthResponse()
    {
        var registerUserDto = _fixture.Build<RegisterUserDTO>().Create();
        var authResponseDtoExpected = new AuthResponseDTO
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Email = registerUserDto.Email
        };
        var authResponseDtoResult = await _userService.Register(registerUserDto);

        Assert.Equal(authResponseDtoExpected, authResponseDtoResult);
    }

    [Fact]
    public async Task Register_ShouldSaveUserInDatabase()
    {
        var registerUserDto = _fixture.Build<RegisterUserDTO>().Create();
        var authResponseDtoResult = await _userService.Register(registerUserDto);
        var authResponseDtoExpected =
            await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == authResponseDtoResult.Email);

        Assert.NotNull(authResponseDtoResult);
        Assert.Equal(authResponseDtoExpected, authResponseDtoExpected);
    }

    #endregion
}