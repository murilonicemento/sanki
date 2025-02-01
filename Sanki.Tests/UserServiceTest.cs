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

        var passwordService = new PasswordService();
        _userService = new UserService(_sankiContext, passwordService);
    }

    #region Register

    [Fact]
    public async Task Register_PersonAlreadyExist_ShouldBeInvalidOperationException()
    {
        var registerUserDto = _fixture.Build<RegisterUserRequestDTO>().Create();

        await _userService.RegisterAsync(registerUserDto);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _userService.RegisterAsync(registerUserDto);
        });
    }

    [Fact]
    public async Task Register_ShouldReturnValidRegisterResponse()
    {
        var registerUserDto = _fixture.Build<RegisterUserRequestDTO>().Create();
        var registerUserResponseDtoExpected = new RegisterUserResponseDTO
        {
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName,
            Email = registerUserDto.Email
        };
        var registerUserResponseDtoResult = await _userService.RegisterAsync(registerUserDto);

        Assert.Equal(registerUserResponseDtoExpected, registerUserResponseDtoResult);
    }

    [Fact]
    public async Task Register_ShouldSaveUserInDatabase()
    {
        var registerUserDto = _fixture.Build<RegisterUserRequestDTO>().Create();
        var authResponseDtoResult = await _userService.RegisterAsync(registerUserDto);
        var authResponseDtoExpected =
            await _sankiContext.Users.FirstOrDefaultAsync(user => user.Email == authResponseDtoResult.Email);

        Assert.NotNull(authResponseDtoResult);
        Assert.Equal(authResponseDtoExpected, authResponseDtoExpected);
    }

    #endregion
}