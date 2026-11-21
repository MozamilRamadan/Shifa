using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Authentication;

namespace Shifa.UnitTests.Authentication;

public class RefreshTokenServiceTests
{
    private readonly RefreshTokenService _service = new();

    [Fact]
    public void GenerateRefreshToken_ShouldReturnValidRandomToken()
    {
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(token1));
        Assert.NotEqual(token1, token2);

        var bytes = Convert.FromBase64String(token1);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void HashToken_ShouldReturnConsistentHash()
    {
        var token = "test-refresh-token";

        var hash1 = _service.HashToken(token);
        var hash2 = _service.HashToken(token);

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(token, hash1);
    }
}