namespace Sanki.Services.Contracts.DTO;

public class AuthResponseDTO
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? Token { get; set; }

    public string RefreshToken { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != typeof(AuthResponseDTO)) return false;

        AuthResponseDTO authResponseDto = (AuthResponseDTO)obj;

        return FirstName == authResponseDto.FirstName && LastName == authResponseDto.LastName &&
               Email == authResponseDto.Email;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, Email);
    }
}