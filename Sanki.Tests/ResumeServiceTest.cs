using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services;

namespace Sanki.Tests;

public class ResumeServiceTest
{
    private readonly Fixture _fixture;
    private readonly SankiContext _sankiContext;
    private readonly ResumeService _resumeService;
    private readonly JwtService _jwtService;

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

        var options = new DbContextOptionsBuilder<SankiContext>()
            .UseInMemoryDatabase(databaseName: "SankiTestDatabase")
            .Options;
        _sankiContext = new SankiContext(options);

        _sankiContext.Database.EnsureCreated();

        _resumeService = new ResumeService(_jwtService, _sankiContext);
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
            .With(user => user.Resumes, new List<Resume>())
            .With(user => user.Email, string.Empty)
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _resumeService.GetResumesByUserAsync(loginUserResponseDto.Token);
        });
    }

    #endregion
}