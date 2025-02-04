using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IResumeService
{
    public Task<List<ResumeDTO>> GetResumesByUserAsync(string token);
    public Task AddResumeAsync(ResumeRequestDTO resumeRequestDto, string token);
}