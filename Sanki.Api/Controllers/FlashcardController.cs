using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sanki.Api.Validations.Interfaces;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;
    private readonly ITokenValidator _tokenValidator;

    public FlashcardController(IFlashcardService flashcardService, ITokenValidator tokenValidator)
    {
        _flashcardService = flashcardService;
        _tokenValidator = tokenValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<FlashcardResponseDTO>>> GetFlashcards()
    {
        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);

        if (isValid is false)
        {
            return Problem("Token not given.", statusCode: 400);
        }

        try
        {
            var flashcards = await _flashcardService.GetFlashcardsByUserAsync(token);

            return Ok(flashcards);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
        catch (SecurityTokenException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
    }
}