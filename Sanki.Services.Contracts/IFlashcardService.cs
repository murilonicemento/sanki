using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IFlashcardService
{
    public Task<List<FlashcardResponseDTO>> GetFlashcardsByUserAsync(string token);

    public Task GenerateFlashcardsAsync(Guid resumeId, string token);
}