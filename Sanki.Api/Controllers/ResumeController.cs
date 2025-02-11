using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ITokenValidator _tokenValidator;

    public ResumeController(
        IResumeService resumeService,
        IModelStateValidator modelStateValidator,
        ITokenValidator tokenValidator)
    {
        _resumeService = resumeService;
        _modelStateValidator = modelStateValidator;
        _tokenValidator = tokenValidator;
    }

    [HttpGet]
    public async Task<ActionResult<ResumeResponseDTO>> GetResumes()
    {
        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);

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
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }

    [HttpPost]
    public async Task<ActionResult> AddResume(AddResumeRequestDTO addResumeRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            await _resumeService.AddResumeAsync(addResumeRequestDto, token);

            return Ok();
        }
        catch (UnauthorizedAccessException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
        catch (SecurityTokenException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateResume(UpdateResumeRequestDTO updateResumeRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            var resume = await _resumeService.UpdateResumeAsync(updateResumeRequestDto, token);

            return Ok(resume);
        }
        catch (KeyNotFoundException exception)
        {
            return Problem(exception.Message, statusCode: 404);
        }
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteResume(Guid id)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        var (isValid, token) = _tokenValidator.ValidateToken(HttpContext);
        ;

        if (!isValid) return Problem("Token was not given.", statusCode: 400);

        try
        {
            await _resumeService.DeleteResumeAsync(id, token);

            return Ok();
        }
        catch (KeyNotFoundException exception)
        {
            return Problem(exception.Message, statusCode: 404);
        }
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }
}