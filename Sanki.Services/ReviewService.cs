using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Entities.Enums;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class ReviewService : IReviewService
{
    private readonly IJwtService _jwtService;
    private readonly SankiContext _sankiContext;

    public ReviewService(IJwtService jwtService, SankiContext sankiContext)
    {
        _jwtService = jwtService;
        _sankiContext = sankiContext;
    }

    public async Task SaveNextReviewDateAsync(SaveReviewDateRequestDTO saveReviewDateRequestDto, string token)
    {
        var principal = GetPrincipal(token);

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            throw new UnauthorizedAccessException("User is not authorized.");
        }

        var flashcard = await _sankiContext.Flashcards
            .FirstOrDefaultAsync(flashcard =>
                flashcard.Id == saveReviewDateRequestDto.FlashcardId && flashcard.UserId == userId);

        if (flashcard is null) throw new UnauthorizedAccessException("User is not authorized to access resource.");

        var flashcardNote = saveReviewDateRequestDto.FlashcardNote;
        var reviewDate = saveReviewDateRequestDto.ReviewDate ?? DateTime.Today;

        switch (flashcardNote)
        {
            case < 3:
                flashcard.Status = StatusOptions.Study.ToString();

                await _sankiContext.SaveChangesAsync();
                break;
            case <= 5:
            {
                var ef = 2.5 + (0.1 - (5 - flashcardNote) * (0.08 + (5 - flashcardNote) * 0.02));
                var nextReviewDate = DateTime.Today.AddDays(reviewDate.Day * ef);
                var currentReview =
                    await _sankiContext.Reviews.FirstOrDefaultAsync(review => review.FlashcardId == flashcard.Id);

                if (currentReview is null)
                {
                    await _sankiContext.Reviews.AddAsync(new Review
                    {
                        ReviewDate = nextReviewDate,
                        FlashcardId = flashcard.Id
                    });
                }
                else
                {
                    currentReview.ReviewDate = nextReviewDate;
                }

                flashcard.Status = StatusOptions.Review.ToString();

                await _sankiContext.SaveChangesAsync();

                break;
            }
        }
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        return principal;
    }
}