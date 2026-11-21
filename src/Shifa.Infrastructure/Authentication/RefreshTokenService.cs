using System.Security.Cryptography;
using System.Text;
using Shifa.Application.Interfaces.Authentication;

namespace Shifa.Infrastructure.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}