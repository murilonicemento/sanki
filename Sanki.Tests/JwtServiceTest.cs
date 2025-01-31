using AutoFixture;
using Microsoft.Extensions.Configuration;
using Sanki.Entities;
using Sanki.Services;

namespace Sanki.Tests;

public class JwtServiceTest
{
    private readonly Fixture _fixture;
    private readonly JwtService _jwtService;

    public JwtServiceTest()
    {
        _fixture = new Fixture();

        var inMemoryConfig = new Dictionary<string, string>
        {
            { "Jwt:Key", "12345678901234567890123456789012" },
            { "Jwt:EXPIRATION_MINUTES", "720" },
            { "Jwt:Issuer", "test-issuer" },
            { "Jwt:Audience", "test-audience" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateJwt_ShouldNotBeNull()
    {
        var user = _fixture.Build<User>().With(user => user.Resumes, new List<Resume>()).Create();
        var authResponseDto = _jwtService.GenerateJwt(user);

        Assert.NotNull(authResponseDto.Token);
    }
}