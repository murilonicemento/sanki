using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class FlashcardService : IFlashcardService
{
    private readonly IJwtService _jwtService;
    private readonly SankiContext _sankiContext;

    public FlashcardService(IJwtService jwtService, SankiContext sankiContext)
    {
        _jwtService = jwtService;
        _sankiContext = sankiContext;
    }

    public async Task<List<FlashcardResponseDTO>> GetFlashcardsByUserAsync(string token)
    {
        var principal = GetPrincipal(token);

        if (!Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id))
        {
            throw new UnauthorizedAccessException("User is not authorized");
        }

        var flashcards = await _sankiContext.Flashcards
            .Where(flashcard => flashcard.UserId == id)
            .Select(flashcard =>
                new FlashcardResponseDTO
                {
                    Id = flashcard.Id,
                    Question = flashcard.Question,
                    Response = flashcard.Response,
                    Status = flashcard.Status
                }).ToListAsync();

        return flashcards;
    }

    public Task<List<FlashcardResponseDTO>> GenerateFlashcards(string token)
    {
        throw new NotImplementedException();
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        return principal;
    }
}