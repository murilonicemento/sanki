using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class UserServiceTest
{
    private readonly Fixture _fixture;
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IUserRepository _userRepository;

    public UserServiceTest()
    {
        _fixture = new Fixture();

        var passwordService = new PasswordService();

        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepository = _userRepositoryMock.Object;

        _userService = new UserService(passwordService, _userRepository);
    }

    #region Register

    [Fact]
    public async Task Register_PersonAlreadyExist_ShouldBeInvalidOperationException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Flashcards, new List<Flashcard>())
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var registerUserDto = new RegisterUserRequestDTO
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password
        };

        _userRepositoryMock
            .Setup(temp => temp.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

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
        var authResponseDtoExpected = await _userRepository.GetUserByEmailAsync(authResponseDtoResult.Email);

        Assert.NotNull(authResponseDtoResult);
        Assert.Equal(authResponseDtoExpected, authResponseDtoExpected);
    }

    #endregion
}