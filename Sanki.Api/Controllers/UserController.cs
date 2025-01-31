using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanki.Api.Validations.Interfaces;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IModelStateValidator _modelStateValidator;

    public UserController(IUserService userService, IModelStateValidator modelStateValidator)
    {
        _userService = userService;
        _modelStateValidator = modelStateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<RegisterUserResponseDTO>> Register(RegisterUserRequestDTO registerUserRequestDto)
    {
        if (!_modelStateValidator.ValidateModelState(ModelState, out var errorMessages))
        {
            return Problem(errorMessages, statusCode: 400);
        }

        try
        {
            var authResponseDto = await _userService.RegisterAsync(registerUserRequestDto);

            return Ok(authResponseDto);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: 409);
        }
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }
}