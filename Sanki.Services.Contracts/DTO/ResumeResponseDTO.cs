namespace Sanki.Services.Contracts.DTO;

public class ResumeResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
}