using Sanki.Entities;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Repositories.Contracts;

public interface IResumeRepository
{
    public Task<List<Resume>> GetResumesByUserEmailAsync(string email);
    public Task AddResumeAsync(Resume resume);
    public Task<Resume?> GetResumeByIdAndUserEmailAsync(Guid id, string email);
    public Task UpdateResumeAsync(Resume resume, string title, string? content);
    public Task DeleteResumeAsync(Resume resume);
    public Task<Resume?> GetResumeByIdAndUserIdAsync(Guid resumeId, Guid userId);
}

