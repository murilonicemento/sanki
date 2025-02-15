using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;

namespace Sanki.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly SankiContext _sankiContext;

    public ReviewRepository(SankiContext sankiContext)
    {
        _sankiContext = sankiContext;
    }

    public async Task<Review?> GetReviewByFlashcardId(Guid id)
    {
        return await _sankiContext.Reviews.FirstOrDefaultAsync(review => review.FlashcardId == id);
    }

    public async Task AddReview(Review review)
    {
        await _sankiContext.Reviews.AddAsync(review);
    }

    public async Task UpdateReviewDate(Review review, DateTime nextReviewDate)
    {
        review.ReviewDate = nextReviewDate;

        await _sankiContext.SaveChangesAsync();

    }
}

