using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sanki.Api.Validations.Interfaces;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly IResumeService _resumeService;
    private readonly IModelStateValidator _modelStateValidator;

    public ResumeController(IResumeService resumeService, IModelStateValidator modelStateValidator)
    {
        _resumeService = resumeService;
        _modelStateValidator = modelStateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ResumeDTO>> GetResumes()
    {
        var accessToken = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(accessToken)) return Problem("Invalid request.", statusCode: 400);

        try
        {
            var resumes = await _resumeService.GetResumesByUserAsync(accessToken);

            return Ok(resumes);
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

    [HttpPost]
    public async Task<ActionResult> AddResume(AddResumeRequestDTO addResumeRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        if (token.IsNullOrEmpty()) return Problem("Token was not given.", statusCode: 400);

        try
        {
            await _resumeService.AddResumeAsync(addResumeRequestDto, token);

            return NoContent();
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