using Microsoft.AspNetCore.Mvc;
using Sanki.Api.Validations.Interfaces;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IModelStateValidator _modelStateValidator;
    private readonly ITokenValidator _tokenValidator;
    private readonly IReviewService _reviewService;

    public ReviewController(IModelStateValidator modelStateValidator, ITokenValidator tokenValidator,
        IReviewService reviewService)
    {
        _modelStateValidator = modelStateValidator;
        _tokenValidator = tokenValidator;
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<ActionResult> SaveReviewDate(SaveReviewDateRequestDTO saveReviewDateRequestDto)
    {
        var isModelStateValid = _modelStateValidator.ValidateModelState(ModelState, out var errorMessages);

        if (!isModelStateValid) return Problem(errorMessages, statusCode: 400);

        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);

        if (!isValid) return Problem("Token not given.", statusCode: 400);

        try
        {
            await _reviewService.SaveNextReviewDateAsync(saveReviewDateRequestDto, token);

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}