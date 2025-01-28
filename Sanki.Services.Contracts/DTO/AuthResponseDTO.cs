namespace Sanki.Services.Contracts.DTO;

public class AuthResponseDTO
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? Token { get; set; }

    public string RefreshToken { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }
}