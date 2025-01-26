using Sanki.Entities;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IJwtService
{
    public AuthResponseDTO GenerateJwt(User user);
}