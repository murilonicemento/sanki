namespace Sanki.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public byte[] Salt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiration { get; set; }

    public virtual ICollection<Flashcard> Flashcards { get; set; }

    public virtual ICollection<Resume> Resumes { get; set; }
}