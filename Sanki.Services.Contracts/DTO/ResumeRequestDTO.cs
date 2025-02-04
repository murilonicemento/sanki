using System.ComponentModel.DataAnnotations;

namespace Sanki.Services.Contracts.DTO;

public class ResumeRequestDTO
{
    [Required(ErrorMessage = "Resume title can't be blank.")]
    public string Title { get; set; }

    public string? Content { get; set; }
}