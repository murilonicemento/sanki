using AutoFixture;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class UserServiceTest
{
    private readonly Fixture _fixture;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        _fixture = new Fixture();

        var sankiContextMock = new DbContextMock<SankiContext>(new DbContextOptionsBuilder<SankiContext>().Options);

        sankiContextMock.CreateDbSetMock(context => context.Users, new List<User>());

        _jwtServiceMock = new Mock<IJwtService>();
        _userService = new UserService(sankiContextMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Register_PersonAlreadyExist_ToBeInvalidOperationException()
    {
        var registerUserDto = _fixture.Build<RegisterUserDTO>().Create();

        await _userService.Register(registerUserDto);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _userService.Register(registerUserDto);
        });
    }
}