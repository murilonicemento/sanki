using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class ResumeService : IResumeService
{
    private readonly IJwtService _jwtService;
    private readonly SankiContext _sankiContext;

    public ResumeService(IJwtService jwtService, SankiContext sankiContext)
    {
        _jwtService = jwtService;
        _sankiContext = sankiContext;
    }

    public async Task<List<ResumeResponseDTO>> GetResumesByUserAsync(string token)
    {
        var principal = GetPrincipal(token);
        var email = GetEmail(principal);

        var resumes = await _sankiContext.Resumes
            .Where(resume => resume.User.Email == email)
            .Select(resume => new ResumeResponseDTO { Id = resume.Id, Title = resume.Title, Content = resume.Content })
            .ToListAsync();

        return resumes;
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

        await _sankiContext.Resumes.AddAsync(resume);
        await _sankiContext.SaveChangesAsync();
    }

    public async Task<ResumeResponseDTO> UpdateResumeAsync(UpdateResumeRequestDTO updateResumeRequestDto, string token)
    {
        var principal = GetPrincipal(token);
        var email = GetEmail(principal);
        var resume = _sankiContext.Resumes
            .FirstOrDefault(resume => resume.Id == updateResumeRequestDto.Id && resume.User.Email == email);

        if (resume is null) throw new KeyNotFoundException("Resume not found for current user.");

        resume.Title = updateResumeRequestDto.Title;
        resume.Content = updateResumeRequestDto.Content;

        await _sankiContext.SaveChangesAsync();

        return new ResumeResponseDTO
        {
            Id = resume.Id,
            Title = resume.Title,
            Content = resume.Content
        };
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