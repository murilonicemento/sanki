namespace Sanki.Services.Contracts.DTO;

public class ResumeResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != typeof(ResumeResponseDTO)) return false;

        var resumeResponseDto = (ResumeResponseDTO)obj;

        return Id == resumeResponseDto.Id && Title == resumeResponseDto.Title && Content == resumeResponseDto.Content;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Title, Content);
    }
}