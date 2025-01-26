namespace Sanki.Entities;

public class Review
{
    public Guid Id { get; set; }

    public DateTime ReviewDate { get; set; }

    public Guid FlashcardId { get; set; }

    public virtual Flashcard Flashcard { get; set; }
}