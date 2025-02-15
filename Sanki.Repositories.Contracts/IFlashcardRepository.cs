using Sanki.Entities;
using Sanki.Entities.Enums;

namespace Sanki.Repositories.Contracts;

public interface IFlashcardRepository
{
    public Task<List<Flashcard>> GetFlashcards(Guid id);
    public Task<Flashcard?> GetFlashcardByIdAndUserIdAsync(Guid flashcardId, Guid userId);
    public Task UpdateFlashcardStatusAsync(Flashcard flashcard, StatusOptions status);
    public Task AddFlashcardListAsync(List<Flashcard> flashcards);
}
