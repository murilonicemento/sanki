using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Entities.Enums;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;

namespace Sanki.Repositories;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly SankiContext _sankiContext;

    public FlashcardRepository(SankiContext sankiContext)
    {
        _sankiContext = sankiContext;
    }

    public async Task<List<Flashcard>> GetFlashcards(Guid id)
    {
        return await _sankiContext.Flashcards
            .Where(flashcard => flashcard.UserId == id)
            .ToListAsync();
    }

    public async Task<Flashcard?> GetFlashcardByIdAndUserIdAsync(Guid flashcardId, Guid userId)
    {
        return await _sankiContext.Flashcards
        .FirstOrDefaultAsync(flashcard =>
               flashcard.Id == flashcardId && flashcard.UserId == userId);
    }

    public async Task UpdateFlashcardStatusAsync(Flashcard flashcard, StatusOptions status)
    {
        flashcard.Status = status.ToString();

        await _sankiContext.SaveChangesAsync();
    }

    public async Task AddFlashcardListAsync(List<Flashcard> flashcards)
    {
        await _sankiContext.Flashcards.AddRangeAsync(flashcards);
        await _sankiContext.SaveChangesAsync();
    }
}

