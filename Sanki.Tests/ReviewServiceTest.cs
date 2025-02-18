using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Tests;

public class ReviewServiceTest
{
    private readonly Fixture _fixture;
    private readonly Mock<IFlashcardRepository> _flashcardRespositoryMock;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly IReviewRepository _reviewRepository;
    private readonly ReviewService _reviewService;
    private readonly JwtService _jwtService;

    public ReviewServiceTest()
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

        _flashcardRespositoryMock = new Mock<IFlashcardRepository>();
        _flashcardRepository = _flashcardRespositoryMock.Object;
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _reviewRepository = _reviewRepositoryMock.Object;

        _reviewService = new ReviewService(_jwtService, _flashcardRepository, _reviewRepository);
    }


    [Fact]
    public async Task SaveNextReviewDateAsync_InvalidToken_ShouldThrowSecurityTokenException()
    {
        const string token = "invalid-token";
        var review = _fixture.Build<SaveReviewDateRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _reviewService.SaveNextReviewDateAsync(review, token);
        });
    }

    [Fact]
    public async Task SaveNextReviewDateAsync_NotAuthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Email, string.Empty)
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var review = _fixture.Build<SaveReviewDateRequestDTO>().Create();

        await Assert.ThrowsAsync<SecurityTokenException>(async () =>
        {
            await _reviewService.SaveNextReviewDateAsync(review, loginUserResponseDto.Token);
        });
    }

    [Fact]
    public async Task SaveNextReviewDateAsync_FlashcardNoteLessThenThree_ShouldBeStatusPending()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var review = _fixture.Build<SaveReviewDateRequestDTO>()
            .With(review => review.FlashcardNote, 2)
            .Create();

        await _reviewService.SaveNextReviewDateAsync(review, loginUserResponseDto.Token);

        var flashcard = await _flashcardRepository.GetFlashcardByIdAndUserIdAsync(review.FlashcardId, user.Id);

        Assert.True(flashcard.Status == "Pending");
    }

    [Fact]
    public async Task SaveNextReviewDateAsync_FlashcardNoteGreaterThenFive_ShouldBeStatusReview()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var review = _fixture.Build<SaveReviewDateRequestDTO>()
            .With(review => review.FlashcardNote, 5)
            .With(review => review.ReviewDate, DateTime.Now)
            .Create();

        var ef = 2.5 + (0.1 - (5 - review.FlashcardNote) * (0.08 + (5 - review.FlashcardNote) * 0.02));

        var nextReviewDate = DateTime.Today.AddDays(review.ReviewDate.Value.Day * ef);

        await _reviewService.SaveNextReviewDateAsync(review, loginUserResponseDto.Token);

        var flashcard = await _flashcardRepository.GetFlashcardByIdAndUserIdAsync(review.FlashcardId, user.Id);

        Assert.True(flashcard.Status == "Review");
    }

    [Fact]
    public async Task SaveNextReviewDateAsync_NextReviewDate_ShouldBeValid()
    {
        var user = _fixture.Build<User>()
            .With(user => user.Resumes, new List<Resume>())
            .Create();
        var loginUserResponseDto = _jwtService.GenerateJwt(user);
        var review = _fixture.Build<SaveReviewDateRequestDTO>()
            .With(review => review.FlashcardNote, 5)
            .With(review => review.ReviewDate, DateTime.Now)
            .Create();

        var ef = 2.5 + (0.1 - (5 - review.FlashcardNote) * (0.08 + (5 - review.FlashcardNote) * 0.02));

        var nextReviewDate = DateTime.Today.AddDays(review.ReviewDate.Value.Day * ef);

        await _reviewService.SaveNextReviewDateAsync(review, loginUserResponseDto.Token);

        var flashcard = await _flashcardRepository.GetFlashcardByIdAndUserIdAsync(review.FlashcardId, user.Id);

        Assert.True(flashcard.Review.ReviewDate == nextReviewDate);
    }
}