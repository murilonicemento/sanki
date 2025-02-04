using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IResumeService
{
    public Task<List<ResumeResponseDTO>> GetResumesByUserAsync(string token);
    public Task AddResumeAsync(ResumeRequestDTO resumeRequestDto, string token);
    public Task<ResumeResponseDTO> UpdateResumeAsync(ResumeRequestDTO resumeRequestDto, string token);
}