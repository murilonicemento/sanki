using Sanki.Entities;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IJwtService
{
    public LoginUserResponseDTO GenerateJwt(User user);
}