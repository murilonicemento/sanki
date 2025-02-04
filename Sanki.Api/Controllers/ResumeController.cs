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
    public async Task<ActionResult<ResumeResponseDTO>> GetResumes()
    {
        var (isValid, token) = ValidateToken();

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            var resumes = await _resumeService.GetResumesByUserAsync(token);

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
    public async Task<ActionResult> AddResume(ResumeRequestDTO resumeRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var (isValid, token) = ValidateToken();

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            await _resumeService.AddResumeAsync(resumeRequestDto, token);

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

    [HttpPut]
    public async Task<ActionResult> UpdateResume(ResumeRequestDTO resumeRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var (isValid, token) = ValidateToken();

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            var resume = await _resumeService.UpdateResumeAsync(resumeRequestDto, token);

            return Ok(resume);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private (bool isValid, string token) ValidateToken()
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        return token.IsNullOrEmpty() ? (false, string.Empty) : (true, token);
    }
}