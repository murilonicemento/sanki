using System.ComponentModel.DataAnnotations;
using Sanki.Entities;

namespace Sanki.Services.Contracts.DTO;

public class RegisterUserDTO
{
    [Required(ErrorMessage = "First name can't be blank.")]
    [MaxLength(50, ErrorMessage = "First name max length is 50 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name can't be blank.")]
    [MaxLength(50, ErrorMessage = "Last name max length is 50 characters.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "First name can't be blank.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "First name can't be blank.")]
    [MinLength(8, ErrorMessage = "Password to short.")]
    [MaxLength(255, ErrorMessage = "First name max length is 255 characters.")]
    public string Password { get; set; }
}