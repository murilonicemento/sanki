using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IReviewService
{
    public Task SaveNextReviewDateAsync(SaveReviewDateRequestDTO saveReviewDateRequestDto, string token);
}