using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class ResumeServiceTest
{
    private readonly Fixture _fixture;
    private readonly ResumeService _resumeService;
    private readonly JwtService _jwtService;
    private readonly Mock<IResumeRepository> _resumeRepositoryMock;
    private readonly IResumeRepository _resumeRepository;

    public ResumeServiceTest()
    {
        _fixture = new Fixture();

        var initialData = new Dictionary<string, string>
        {
            { "Jwt:Key", "12345678901234567890123456789012" },
            { "Jwt:EXPIRATION_MINUTES", "720" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(initialData).Build();
        _jwtService = new JwtService(configuration);

        _resumeRepositoryMock = new Mock<IResumeRepository>();
        _resumeRepository = _resumeRepositoryMock.Object;
        
        _resumeService = new ResumeService(_jwtService, _resumeRepository);
    }

    #region GetResumesByUserAsync

    [Fact]
    public async Task GetResumesByUserAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.GetResumesByUserAsync(token);
        });
    }

    [Fact]
    public async Task GetResumesByUserAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GetResumesByUserAsync_UserWithEmptyResumes_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        var resumes = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);

        Assert.Empty(resumes);
    }

    [Fact]
    public async Task GetResumesByUserAsync_UserWithResumes_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await _resumeService.AddResumeAsync(addResumeDto, loginUserResponseDto.Token);

        var resumes = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);

        Assert.NotEmpty(resumes);
    }

    #endregion

    #region AddResumeAsync

    [Fact]
    public async Task AddResumeAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";
        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.AddResumeAsync(addResumeDto, token);
        });
    }

    [Fact]
    public async Task AddResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Id, Guid.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.AddResumeAsync(addResumeDto, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task AddResumeAsync_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var addResumeDto = _fixture.Build<AddResumeRequestDTO>().Create();

        await _resumeService.AddResumeAsync(addResumeDto, loginUserResponseDto.Token);
        var resumes = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);

        Assert.NotEmpty(resumes);
    }

    #endregion

    #region UpdateResumeAsync

    [Fact]
    public async Task UpdateResumeAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_NotFoundResumeForUser_ShouldKeyNotFoundException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();

        updateResumeRequestDto.Id = Guid.NewGuid();

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _resumeService.UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task UpdateResumeAsync_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var updateResumeRequestDto = _fixture.Build<UpdateResumeRequestDTO>().Create();
        var updateResumeExpected = await _resumeService
            .UpdateResumeAsync(updateResumeRequestDto, loginUserResponseDto.Token);
        var resume = await _resumeRepository.GetResumeByIdAndUserIdAsync(updateResumeExpected.Id, user.Id);
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

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(id, token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var id = Guid.NewGuid();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_ResumeAlreadyDeleted_ShouldThrowKeyNotFoundException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var addResume = _fixture.Build<AddResumeRequestDTO>().Create();

        await _resumeService.AddResumeAsync(addResume, loginUserResponseDto.Token);

        var resume = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);
        var id = resume[0].Id;

        await _resumeService.DeleteResumeAsync(id, loginUserResponseDto.Token);

        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await _resumeService.DeleteResumeAsync(id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task DeleteResumeAsync_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var addResume = _fixture.Build<AddResumeRequestDTO>().Create();

        await _resumeService.AddResumeAsync(addResume, loginUserResponseDto.Token);

        var resume = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);
        var id = resume[0].Id;

        await _resumeService.DeleteResumeAsync(id, loginUserResponseDto.Token);

        var allResumes = await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);

        Assert.Empty(allResumes);
    }

    #endregion
}