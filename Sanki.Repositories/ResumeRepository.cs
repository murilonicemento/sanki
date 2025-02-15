using Microsoft.EntityFrameworkCore;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly SankiContext _sankiContext;

    public ResumeRepository(SankiContext sankiContext)
    {
        _sankiContext = sankiContext;
    }

    public async Task<List<Resume>> GetResumesByUserEmailAsync(string email)
    {
        return await _sankiContext.Resumes
            .Where(resume => resume.User.Email == email)
            .ToListAsync();
    }

    public async Task AddResumeAsync(Resume resume)
    {
        await _sankiContext.Resumes.AddAsync(resume);
        await _sankiContext.SaveChangesAsync();
    }

    public async Task<Resume?> GetResumeByIdAndUserEmailAsync(Guid id, string email)
    {
        return await _sankiContext.Resumes
            .FirstOrDefaultAsync(resume => resume.Id == id && resume.User.Email == email);
    }

    public async Task UpdateResumeAsync(Resume resume, string title, string? content)
    {
        resume.Title = title;
        resume.Content = content;

        await _sankiContext.SaveChangesAsync();
    }

    public async Task DeleteResumeAsync(Resume resume)
    {
        _sankiContext.Resumes.Remove(resume);

        await _sankiContext.SaveChangesAsync();
    }


    public Task<Resume?> GetResumeByIdAndUserIdAsync(Guid resumeId, Guid userId)
    {
        return _sankiContext.Resumes
            .FirstOrDefaultAsync(resume => resume.Id == resumeId && resume.UserId == userId);
    }
}

