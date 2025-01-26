namespace Sanki.Entities;

public class Resume
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string? Content { get; set; }

    public Guid UserId { get; set; }

    public virtual ICollection<Flashcard> Flashcards { get; set; }

    public virtual User User { get; set; }
}