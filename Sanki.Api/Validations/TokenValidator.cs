using Microsoft.IdentityModel.Tokens;
using Sanki.Api.Validations.Interfaces;

namespace Sanki.Api.Validations;

public class TokenValidator : ITokenValidator
{
    public (bool isValid, string token) ValidateToken(HttpContext httpContext)
    {
        var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

        return token.IsNullOrEmpty() ? (false, string.Empty) : (true, token);
    }
}