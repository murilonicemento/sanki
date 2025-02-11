namespace Sanki.Entities;

public class Flashcard
{
    public Guid Id { get; set; }

    public string Question { get; set; }

    public string Response { get; set; }

    public string Status { get; set; }

    public Guid UserId { get; set; }

    public Guid ResumeId { get; set; }

    public virtual User User { get; set; }

    public virtual Resume Resume { get; set; }

    public virtual ICollection<Review> Reviews { get; set; }
}