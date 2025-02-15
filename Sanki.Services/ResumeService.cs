using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Repositories.Contracts;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class ResumeService : IResumeService
{
    private readonly IJwtService _jwtService;
    private readonly IResumeRepository _resumeRepository;

    public ResumeService(IJwtService jwtService, IResumeRepository resumeRepository)
    {
        _jwtService = jwtService;
        _resumeRepository = resumeRepository;
    }

    public async Task<List<ResumeResponseDTO>> GetResumesByUserAsync(string token)
    {
        var principal = GetPrincipal(token);
        var email = GetEmail(principal);
        var resumes = await _resumeRepository.GetResumesByUserEmailAsync(email);

        return resumes
            .Select(resume => new ResumeResponseDTO { Id = resume.Id, Title = resume.Title, Content = resume.Content })
            .ToList();
    }

    public async Task AddResumeAsync(AddResumeRequestDTO addResumeRequestDto, string token)
    {
        var principal = GetPrincipal(token);

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            throw new UnauthorizedAccessException("User is not authorized.");
        }

        var resume = new Resume
        {
            Title = addResumeRequestDto.Title,
            Content = addResumeRequestDto.Content,
            UserId = id
        };

        await _resumeRepository.AddResumeAsync(resume);
    }

    public async Task<ResumeResponseDTO> UpdateResumeAsync(UpdateResumeRequestDTO updateResumeRequestDto, string token)
    {
        var principal = GetPrincipal(token);
        var email = GetEmail(principal);
        var resume = await _resumeRepository.GetResumeByIdAndUserEmailAsync(updateResumeRequestDto.Id, email)
            ?? throw new KeyNotFoundException("Resume not found for current user.");

        await _resumeRepository.UpdateResumeAsync(resume, updateResumeRequestDto.Title, updateResumeRequestDto.Content);

        return new ResumeResponseDTO
        {
            Id = resume.Id,
            Title = resume.Title,
            Content = resume.Content
        };
    }

    public async Task DeleteResumeAsync(Guid id, string token)
    {
        var principal = GetPrincipal(token);
        var email = GetEmail(principal);

        var resume = await _resumeRepository.GetResumeByIdAndUserEmailAsync(id, email)
            ?? throw new KeyNotFoundException("Resume already deleted.");

        await _resumeRepository.DeleteResumeAsync(resume);
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        return principal;
    }

    private static string GetEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (email is null) throw new UnauthorizedAccessException("User is not authorized.");

        return email;
    }
}