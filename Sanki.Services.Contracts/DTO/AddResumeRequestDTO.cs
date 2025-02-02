using System.ComponentModel.DataAnnotations;

namespace Sanki.Services.Contracts.DTO;

public class AddResumeRequestDTO
{
    [Required(ErrorMessage = "Resume title can't be blank.")]
    public string Title { get; set; }

    public string? Content { get; set; }

    [Required(ErrorMessage = "Token can't be blank.")]
    public string Token { get; set; }
}