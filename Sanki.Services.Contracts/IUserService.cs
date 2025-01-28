using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IUserService
{
    public Task<AuthResponseDTO> Register(RegisterUserDTO registerUserDto);
}