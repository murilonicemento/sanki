using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IAuthService
{
    public Task<LoginUserResponseDTO?> LoginAsync(LoginUserRequestDTO loginUserRequestDto);
    public Task<LoginUserResponseDTO> GenerateNewAccessTokenAsync(TokenRequestDTO tokenRequestDto);
}