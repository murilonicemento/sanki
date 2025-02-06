using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IResumeService
{
    public Task<List<ResumeResponseDTO>> GetResumesByUserAsync(string token);
    public Task AddResumeAsync(AddResumeRequestDTO addResumeRequestDto, string token);
    public Task<ResumeResponseDTO> UpdateResumeAsync(UpdateResumeRequestDTO updateResumeRequestDto, string token);
    public Task DeleteResumeAsync(Guid id, string token);
}