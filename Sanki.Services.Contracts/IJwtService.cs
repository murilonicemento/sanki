using System.Security.Claims;
using Sanki.Entities;
using Sanki.Services.Contracts.DTO;

namespace Sanki.Services.Contracts;

public interface IJwtService
{
    public LoginUserResponseDTO GenerateJwt(User user);
    public ClaimsPrincipal? GetPrincipalFromJwt(string token);
}