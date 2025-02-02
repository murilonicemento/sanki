using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sanki.Api.Validations.Interfaces;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IModelStateValidator _modelStateValidator;

    public AuthController(IAuthService authService, IModelStateValidator modelStateValidator)
    {
        _authService = authService;
        _modelStateValidator = modelStateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<LoginUserResponseDTO>> Login(LoginUserRequestDTO loginUserRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        try
        {
            var loginUserResponseDto = await _authService.LoginAsync(loginUserRequestDto);

            return Ok(loginUserResponseDto);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
    }

    [HttpPost("GenerateNewToken")]
    public async Task<ActionResult<LoginUserResponseDTO>> GenerateNewToken(TokenRequestDTO? tokenRequest)
    {
        if (tokenRequest is null) return Problem("Invalid request.", statusCode: 400);

        try
        {
            var loginResponseDto = await _authService.GenerateNewAccessTokenAsync(tokenRequest);

            return loginResponseDto;
        }
        catch (SecurityTokenException exception)
        {
            return Problem(exception.Message, statusCode: 401);
        }
    }
}