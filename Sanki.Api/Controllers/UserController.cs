using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanki.Services.Contracts;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterUserDTO registerUserDto)
    {
        if (!ModelState.IsValid)
        {
            var errorsList = ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage)
                .ToList();
            var errorsString = string.Join(" | ", errorsList);

            return Problem(errorsString, statusCode: 400);
        }

        try
        {
            var authResponseDto = await _userService.Register(registerUserDto);

            return Ok(authResponseDto);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(exception.Message, statusCode: 400);
        }
        catch (DbUpdateException)
        {
            return Problem("An error occurred. Contact the system admin.", statusCode: 500);
        }
    }
}