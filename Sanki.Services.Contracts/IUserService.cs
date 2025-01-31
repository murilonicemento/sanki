using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IUserService
{
    public Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserRequestDTO registerUserRequestDto);
}