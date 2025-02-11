namespace Sanki.Api.Validations.Interfaces;

public interface ITokenValidator
{
    public (bool isValid, string token) ValidateToken(HttpContext httpContext);
}