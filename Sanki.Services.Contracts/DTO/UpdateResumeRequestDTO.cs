using System.ComponentModel.DataAnnotations;

namespace Sanki.Services.Contracts.DTO;

public class UpdateResumeRequestDTO
{
    [Required(ErrorMessage = "Resume identifier can't be blank.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Resume title can't be blank.")]
    public string Title { get; set; }

    public string? Content { get; set; }
}