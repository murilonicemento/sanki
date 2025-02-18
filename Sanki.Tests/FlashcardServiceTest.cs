using System.Net;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using Sanki.Configurations;
using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class FlashcardServiceTest
{
    private readonly Fixture _fixture;
    private readonly JwtService _jwtService;
    private readonly HttpClient _httpClient;
    private readonly Mock<IFlashcardRepository> _flashcardRepositoryMock;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly Mock<IResumeRepository> _resumeRepositoryMock;
    private readonly IResumeRepository _resumeRepository;
    private readonly FlashcardService _flashcardService;
    private readonly IOptions<GeminiOptions> _options;

    public FlashcardServiceTest()
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

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"[ ""string 1"", ""string 2"", ""string 3"", ""string 4"" ]"),
        };

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        _httpClient = new HttpClient(httpMessageHandlerMock.Object);

        _options = Options.Create(new GeminiOptions { Url = "http://fakeapi.com", ApiKey = "api-key-12345" });

        _flashcardRepositoryMock = new Mock<IFlashcardRepository>();
        _flashcardRepository = _flashcardRepositoryMock.Object;
        _resumeRepositoryMock = new Mock<IResumeRepository>();
        _resumeRepository = _resumeRepositoryMock.Object;

        _flashcardService =
            new FlashcardService(_jwtService, _httpClient, _options, _flashcardRepository, _resumeRepository);
    }

    #region GetFlashcardsByUserAsync

    [Fact]
    public async Task GetFlashcardsByUserAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _flashcardService.GetFlashcardsByUserAsync(token);
        });
    }

    [Fact]
    public async Task GetFlashcardsByUserAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _flashcardService.GetFlashcardsByUserAsync(loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GetFlashcardsByUserAsync_NotAddedFlashcard_ShouldBeEmptyList()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);

        var flashcards = await _flashcardService.GetFlashcardsByUserAsync(loginUserResponseDto.Token);

        Assert.Empty(flashcards);
    }

    [Fact]
    public async Task GetFlashcardsByUserAsync_AddedFlashcard_ShouldNotBeEmptyList()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var resume = _fixture.Build<Resume>().Create();

        await _resumeRepository.AddResumeAsync(resume);
        await _flashcardService.GenerateFlashcardsAsync(resume.Id, loginUserResponseDto.Token);

        var flashcards = await _flashcardService.GetFlashcardsByUserAsync(loginUserResponseDto.Token);

        Assert.NotEmpty(flashcards);
    }

    #endregion

    #region GenerateFlashcardsAsync

    [Fact]
    public async Task GenerateFlashcardsAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";
        var resume = _fixture.Build<Resume>().Create();

        await _resumeRepository.AddResumeAsync(resume);

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _flashcardService.GenerateFlashcardsAsync(resume.Id, token);
        });
    }

    [Fact]
    public async Task GenerateFlashcardsAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var (loginUserResponseDto, resume) = CreateResumeHandler();

        await _resumeRepository.AddResumeAsync(resume);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _flashcardService.GenerateFlashcardsAsync(resume.Id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GenerateFlashcardsAsync_UserNotAssociatedToResume_ShouldThrowUnauthorizedAccessException()
    {
        var (_, resume) = CreateResumeHandler();

        await _resumeRepository.AddResumeAsync(resume);

        var invalidUser = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var invalidLoginUserResponseDto = _jwtService.GenerateJwt(invalidUser);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await _flashcardService.GenerateFlashcardsAsync(resume.Id, invalidLoginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GenerateFlashcardsAsync_RequestError_ShouldThrowInvalidOperationException()
    {
        var (loginUserResponseDto, resume) = CreateResumeHandler();

        await _resumeRepository.AddResumeAsync(resume);

        var content = new StringContent("null");
        var handlerMock = CreateMockHttpHandler(content, HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handlerMock.Object);

        var service = new FlashcardService(_jwtService, httpClient, _options, _flashcardRepository,
            _resumeRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GenerateFlashcardsAsync(resume.Id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GenerateFlashcardsAsync_ResponseIsNull_ShouldThrowInvalidOperationException()
    {
        var (loginUserResponseDto, resume) = CreateResumeHandler();

        await _resumeRepository.AddResumeAsync(resume);

        var content = new StringContent("null");
        var handlerMock = CreateMockHttpHandler(content);
        var httpClient = new HttpClient(handlerMock.Object);

        var service = new FlashcardService(_jwtService, httpClient, _options, _flashcardRepository,
            _resumeRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GenerateFlashcardsAsync(resume.Id, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task GenerateFlashcardsAsync_CandidatesIsEmpty_ShouldThrowInvalidOperationException()
    {
        var (loginUserResponseDto, resume) = CreateResumeHandler();

        await _resumeRepository.AddResumeAsync(resume);

        var content = new StringContent(new { candidates = new List<Candidate>() }.ToString() ?? string.Empty);
        var handlerMock = CreateMockHttpHandler(content);
        var httpClient = new HttpClient(handlerMock.Object);

        var service = new FlashcardService(_jwtService, httpClient, _options, _flashcardRepository,
            _resumeRepository);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.GenerateFlashcardsAsync(resume.Id, loginUserResponseDto.Token);
        });
    }

    #endregion

    private static Mock<HttpMessageHandler> CreateMockHttpHandler(StringContent content,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content
            });

        return handlerMock;
    }

    private (LoginUserResponseDTO loginUserResponseDto, Resume resume) CreateResumeHandler()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUser = _jwtService.GenerateJwt(user);
        var resume = _fixture.Build<Resume>()
            .With(resume => resume.UserId, user.Id)
            .Create();

        return (loginUser, resume);
    }
}