using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sanki.Entities;
using Sanki.Entities.Enums;
using Sanki.Persistence;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services;

public class FlashcardService : IFlashcardService
{
    private readonly IJwtService _jwtService;
    private readonly SankiContext _sankiContext;
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _geminiOptions;

    public FlashcardService(IJwtService jwtService, SankiContext sankiContext, HttpClient httpClient,
        IOptions<GeminiOptions> geminiOptions)
    {
        _jwtService = jwtService;
        _sankiContext = sankiContext;
        _httpClient = httpClient;
        _geminiOptions = geminiOptions.Value;
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

    public async Task GenerateFlashcardsAsync(Guid resumeId, string token)
    {
        var principal = GetPrincipal(token);

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            throw new UnauthorizedAccessException("User is not authorized");
        }

        var resume = await _sankiContext.Resumes
            .FirstOrDefaultAsync(resume => resume.Id == resumeId && resume.UserId == id);

        if (resume is null) throw new UnauthorizedAccessException("User is not authorized to access the resume.");

        var prompt =
            $"Create question and answer flashcards based on this summary and return a json containing the flashcards where the json should contain the question and answer of the flashcards: \n {resume.Title} \n {resume.Content}";
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };
        var response = await _httpClient
            .PostAsJsonAsync($"{_geminiOptions.Url}{_geminiOptions.ApiKey}",
                requestBody);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("An error occured when generate flashcards. Contact the system admin.");

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

        if (result?.Candidates is not { Count: > 0 })
        {
            var errorMessage = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(errorMessage);
        }

        var flashcards =
            JsonSerializer.Deserialize<List<FlashcardDeserealized>>(result.Candidates[0].Content.Parts[0].Text);

        if (flashcards is null)
            throw new InvalidOperationException("An error occured when generate flashcards. Contact the system admin.");

        var flashcardsList = flashcards.Select(flashcard => new Flashcard
        {
            Question = flashcard.Question,
            Response = flashcard.Response,
            Status = StatusOptions.Pending.ToString(),
            UserId = id,
            ResumeId = resumeId,
        }).ToList();

        await _sankiContext.Flashcards.AddRangeAsync(flashcardsList);
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        return principal;
    }
}