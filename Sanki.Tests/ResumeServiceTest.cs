using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoFixture;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class ResumeServiceTest
{
    private readonly Fixture _fixture;
    private readonly IResumeService _resumeService;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IJwtService _jwtService;
    private readonly Mock<IResumeRepository> _resumeRepositoryMock;
    private readonly IResumeRepository _resumeRepository;

    public ResumeServiceTest()
    {
        _fixture = new Fixture();
        _jwtServiceMock = new Mock<IJwtService>();
        _jwtService = _jwtServiceMock.Object;

        _resumeRepositoryMock = new Mock<IResumeRepository>();
        _resumeRepository = _resumeRepositoryMock.Object;

        _resumeService = new ResumeService(_jwtService, _resumeRepository);
    }

    #region GetResumesByUserAsync

    [Fact]
    public async Task GetResumesByUserAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(null as ClaimsPrincipal);

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.GetResumesByUserAsync(token);
        });
    }

    [Fact]
    public async Task GetResumesByUserAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var loginResponseDto = _fixture.Build<LoginUserResponseDTO>()
            .With(user => user.Email, string.Empty)
            .Create();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([]));

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.GetResumesByUserAsync(loginResponseDto.Token);
        });
    }

    [Fact]
    public async Task GetResumesByUserAsync_UserWithEmptyResumes_ShouldBeValid()
    {
        var user = GenerateUser();
        var loginResponseDto = GenerateLoginResponseDto(user);
        var resumesList = new List<Resume>();
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumesByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(resumesList);

        var resumes = await _resumeService.GetResumesByUserAsync(loginResponseDto.Token);

        Assert.Empty(resumes);
    }

    [Fact]
    public async Task GetResumesByUserAsync_UserWithResumes_ShouldBeValid()
    {
        var user = GenerateUser();
        var loginResponseDto = GenerateLoginResponseDto(user);
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        _resumeRepositoryMock
            .Setup(temp => temp.AddResumeAsync(It.IsAny<Resume>()))
            .Returns(Task.CompletedTask);

        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await _resumeService.AddResumeAsync(addResumeDto, loginResponseDto.Token);

        var resumesList = new List<Resume>
        {
            new()
            {
                Title = addResumeDto.Title,
                Content = addResumeDto.Content
            }
        };

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumesByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(resumesList);

        var resumes = await _resumeService.GetResumesByUserAsync(loginResponseDto.Token);

        Assert.NotEmpty(resumes);
    }

    #endregion

    #region AddResumeAsync

    [Fact]
    public async Task AddResumeAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(null as ClaimsPrincipal);

        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.AddResumeAsync(addResumeDto, token);
        });
    }

    [Fact]
    public async Task AddResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var loginResponseDto = _fixture.Build<LoginUserResponseDTO>()
            .With(user => user.Email, string.Empty)
            .Create();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([]));

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.AddResumeAsync(addResumeDto, loginResponseDto.Token);
        });
    }

    [Fact]
    public async Task AddResumeAsync_ShouldBeValid()
    {
        var user = GenerateUser();
        var loginResponseDto = _fixture.Build<LoginUserResponseDTO>().Create();
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        _resumeRepositoryMock
            .Setup(temp => temp.AddResumeAsync(It.IsAny<Resume>()))
            .Returns(Task.CompletedTask);

        var resumesList = new List<Resume>
        {
            new()
            {
                Title = addResumeDto.Title,
                Content = addResumeDto.Content
            }
        };

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumesByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(resumesList);

        var resumes = await _resumeService.GetResumesByUserAsync(loginResponseDto.Token);

        Assert.NotEmpty(resumes);
    }

    #endregion

    #region UpdateResumeAsync

    [Fact]
    public async Task UpdateResumeAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(null as ClaimsPrincipal);

        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = GenerateUser();

        user.Email = string.Empty;

        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([]));

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_NotFoundResumeForUser_ShouldKeyNotFoundException()
    {
        var user = GenerateUser();
        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumeByIdAndUserEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(null as Resume);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_ShouldBeValid()
    {
        var user = GenerateUser();
        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();
        var resume = new Resume
        {
            Title = updateResumeRequestDto.Title,
            Content = updateResumeRequestDto.Content
        };
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumeByIdAndUserEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(resume);

        _resumeRepositoryMock
            .Setup(temp => temp.UpdateResumeAsync(It.IsAny<Resume>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);


        var updateResumeExpected = await _resumeService
            .UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        var updateResumeResult = new ResumeResponseDTO
        {
            Id = resume.Id,
            Title = resume.Title,
            Content = resume.Content
        };

        Assert.Equal(updateResumeResult, updateResumeExpected);
    }

    #endregion

    #region DeleteResumeAsync

    [Fact]
    public async Task DeleteResumeAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";
        var id = Guid.NewGuid();

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(null as ClaimsPrincipal);

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(id, token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = GenerateUser();

        user.Email = string.Empty;

        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([]));

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_ResumeAlreadyDeleted_ShouldThrowKeyNotFoundException()
    {
        var user = GenerateUser();
        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumeByIdAndUserEmailAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(null as Resume);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(Guid.NewGuid(), loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_ShouldBeValid()
    {
        var user = GenerateUser();
        var loginUserResponseDto = GenerateLoginResponseDto(user);
        var claimsPrincipal = GenerateClaimsPrincipal(user);
        var resume = new Resume
        {
            Id = Guid.NewGuid(),
            Title = "titulo",
            Content = "conteudo"
        };
        var addResume = new AddResumeRequestDTO
        {
            Title = resume.Title,
            Content = resume.Content
        };

        _jwtServiceMock
            .Setup(temp => temp.GetPrincipalFromJwt(It.IsAny<string>()))
            .Returns(claimsPrincipal);
        _resumeRepositoryMock
            .Setup(temp => temp.AddResumeAsync(It.IsAny<Resume>()))
            .Returns(Task.CompletedTask);

        await _resumeService.AddResumeAsync(addResume, loginUserResponseDto.Token);
        
        _resumeRepositoryMock
            .Setup(temp => temp.GetResumeByIdAndUserEmailAsync(It.IsAny<Guid>(),It.IsAny<string>()))
            .ReturnsAsync(resume);
        _resumeRepositoryMock
            .Setup(temp => temp.DeleteResumeAsync(It.IsAny<Resume>()))
            .Returns(Task.CompletedTask);

        await _resumeService.DeleteResumeAsync(resume.Id, loginUserResponseDto.Token);

        _resumeRepositoryMock
            .Setup(temp => temp.GetResumesByUserEmailAsync(It.IsAny<string>()))
            .ReturnsAsync([]);

        var allResumes = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);

        Assert.Empty(allResumes);
    }

    #endregion

    private static ClaimsPrincipal GenerateClaimsPrincipal(User user)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow)
                .ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        ], "mock"));
    }

    private User GenerateUser()
    {
        return _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .With(user => user.Flashcards, new List<Flashcard>())
            .Create();
    }

    private LoginUserResponseDTO GenerateLoginResponseDto(User user)
    {
        return _fixture.Build<LoginUserResponseDTO>()
            .With(userLogin => userLogin.FirstName, user.FirstName)
            .With(userLogin => userLogin.LastName, user.LastName)
            .With(userLogin => userLogin.Email, user.Email)
            .Create();
    }

    private Resume GenerateResume(AddResumeRequestDTO addResumeDto)
    {
        return _fixture.Build<Resume>()
            .With(resume => resume.Title, addResumeDto.Title)
            .With(resume => resume.Content, addResumeDto.Content)
            .With(resume => resume.Flashcards, new List<Flashcard>())
            .Create();
    }
}