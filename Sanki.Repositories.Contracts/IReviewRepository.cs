using Sanki.Entities;

namespace Sanki.Repositories.Contracts;

public interface IReviewRepository
{
    public Task<Review?> GetReviewByFlashcardId(Guid id);
    public Task AddReview(Review review);
    public Task UpdateReviewDate(Review review, DateTime nextReviewDate);
}
