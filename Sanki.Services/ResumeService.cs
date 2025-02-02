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

    public async Task<List<ResumeDTO>> GetResumesByUserAsync(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid json web token.");

        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (email is null) throw new UnauthorizedAccessException("User is not authorized.");

        var resumes = await _sankiContext.Resumes
            .Where(resume => resume.User.Email == email)
            .Select(resume => new ResumeDTO { Title = resume.Title, Content = resume.Content })
            .ToListAsync();

        return resumes;
    }

    public async Task AddResumeAsync(AddResumeRequestDTO addResumeRequestDto)
    {
        var principal = _jwtService.GetPrincipalFromJwt(addResumeRequestDto.Token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        if (!Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id))
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
}