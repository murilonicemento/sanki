using System.ComponentModel.DataAnnotations;

namespace Sanki.Services.Contracts.DTO;

public class SaveReviewDateRequestDTO
{
    [Required(ErrorMessage = "Flashcard identifier can't be blank.")]
    public Guid FlashcardId { get; set; }

    [Required(ErrorMessage = "Flashcard note can't be blank.")]
    [MinLength(0)]
    [MaxLength(5)]
    public int FlashcardNote { get; set; }

    public DateTime? ReviewDate { get; set; }
}