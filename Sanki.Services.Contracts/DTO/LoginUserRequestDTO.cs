using System.ComponentModel.DataAnnotations;

namespace Sanki.Services.Contracts.DTO;

public class LoginUserRequestDTO
{
    [Required(ErrorMessage = "Email can't be blank.")]
    [EmailAddress(ErrorMessage = "Should be a valid email.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password can't be blank.")]
    public string Password { get; set; }
    
    public string Token { get; set; }
}