using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Entities.Enums;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class ReviewService : IReviewService
{
    private readonly IJwtService _jwtService;
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IJwtService jwtService, IFlashcardRepository flashcardRepository, IReviewRepository reviewRepository)
    {
        _jwtService = jwtService;
        _flashcardRepository = flashcardRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task SaveNextReviewDateAsync(SaveReviewDateRequestDTO saveReviewDateRequestDto, string token)
    {
        var principal = GetPrincipal(token);

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException("User is not authorized.");
        }

        var flashcard = await _flashcardRepository.GetFlashcardByIdAndUserIdAsync(saveReviewDateRequestDto.FlashcardId, userId)
            ?? throw new UnauthorizedAccessException("User is not authorized to access resource.");

        var flashcardNote = saveReviewDateRequestDto.FlashcardNote;
        var reviewDate = saveReviewDateRequestDto.ReviewDate ?? DateTime.Today;

        switch (flashcardNote)
        {
            case < 3:
                await _flashcardRepository.UpdateFlashcardStatusAsync(flashcard, StatusOptions.Pending);

                break;
            case <= 5:
                {
                    var ef = 2.5 + (0.1 - (5 - flashcardNote) * (0.08 + (5 - flashcardNote) * 0.02));
                    var nextReviewDate = DateTime.Today.AddDays(reviewDate.Day * ef);
                    var currentReview = await _reviewRepository.GetReviewByFlashcardId(flashcard.Id);

                    if (currentReview is null)
                    {
                        var review = new Review
                        {
                            ReviewDate = nextReviewDate,
                            FlashcardId = flashcard.Id
                        };

                        await _reviewRepository.AddReview(review);
                    }
                    else
                    {
                        await _reviewRepository.UpdateReviewDate(currentReview, nextReviewDate);
                    }

                    await _flashcardRepository.UpdateFlashcardStatusAsync(flashcard, StatusOptions.Review);

                    break;
                }
        }
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token) ?? throw new SecurityTokenException("Invalid token");

        return principal;
    }
}