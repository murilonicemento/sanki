namespace Sanki.Services.Contracts.DTO;

public class FlashcardResponseDTO
{
    public Guid Id { get; set; }

    public string Question { get; set; }

    public string Response { get; set; }

    public string Status { get; set; }
}