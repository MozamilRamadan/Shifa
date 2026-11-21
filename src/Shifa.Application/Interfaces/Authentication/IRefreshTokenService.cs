namespace Shifa.Application.Interfaces.Authentication;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();

    string HashToken(string token);
}