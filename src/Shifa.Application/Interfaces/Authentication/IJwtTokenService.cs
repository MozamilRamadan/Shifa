namespace Shifa.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        int userId,
        string email,
        IEnumerable<string> roles);
}