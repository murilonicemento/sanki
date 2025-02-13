using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Sanki.Configurations;
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

        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
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
                    Status = flashcard.Status,
                    ReviewDate = flashcard.Review.ReviewDate
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

        if (resume is null) throw new UnauthorizedAccessException("User is not authorized to access resource.");

        var prompt =
            $"Generate a list of question-and-answer flashcards based on the following summary. Each flashcard should contain a well-formed question that tests key concepts from the summary and a concise but informative answer. Return only a valid JSON string containing an array of objects, where each object has the fields \"Question\" and \"Response\". Do not format the response as Markdown, code block, or include any additional text. The JSON should not contain any extra indentation, line breaks, or formatting. \n \n Summary: \n Title: {resume.Title} \n Content: {resume.Content} \n";
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

        var content = result.Candidates[0].Content.Parts[0].Text.Replace("```", "").Replace("json", "");
        var flashcards = JsonSerializer.Deserialize<List<FlashcardDeserealized>>(content);
        Console.WriteLine(flashcards);
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
        await _sankiContext.SaveChangesAsync();
    }

    private ClaimsPrincipal GetPrincipal(string token)
    {
        var principal = _jwtService.GetPrincipalFromJwt(token);

        if (principal is null) throw new SecurityTokenException("Invalid token");

        return principal;
    }
}