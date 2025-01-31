using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IAuthService
{
    public Task<LoginUserResponseDTO?> LoginAsync(LoginUserRequestDTO loginUserRequestDto);
}