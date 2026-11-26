using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Shifa.Infrastructure.Authentication;

namespace Shifa.UnitTests.Authentication;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    private const string SecretKey =
        "ThisIsATestSecretKeyWithMoreThan32Characters123456";

    private const string Issuer = "Shifa.API";
    private const string Audience = "Shifa.Client";

    public JwtTokenServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            SecretKey = SecretKey,
            Issuer = Issuer,
            Audience = Audience,
            AccessTokenExpirationMinutes = 15
        });

        _service = new JwtTokenService(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        var token = _service.GenerateAccessToken(
            1,
            "patient@test.com",
            new[] { "Patient" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Contains(Audience, jwt.Audiences);
        Assert.Equal("HS256", jwt.Header.Alg);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserClaims()
    {
        var token = _service.GenerateAccessToken(
            10,
            "doctor@test.com",
            new[] { "Doctor", "Admin" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("10", jwt.Subject);

        Assert.Equal(
            "doctor@test.com",
            jwt.Claims.First(c =>
                c.Type == JwtRegisteredClaimNames.Email).Value);

        Assert.Contains(
            jwt.Claims,
            c => c.Type == ClaimTypes.Role && c.Value == "Doctor");

        Assert.Contains(
            jwt.Claims,
            c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveExpirationDate()
    {
        var beforeGeneration = DateTime.UtcNow;

        var token = _service.GenerateAccessToken(
            1,
            "patient@test.com",
            new[] { "Patient" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(15);

        Assert.True(jwt.ValidTo > beforeGeneration);
        Assert.True(
            Math.Abs((jwt.ValidTo - expectedExpiration).TotalSeconds) < 5);
    }

    [Fact]
    public void GenerateAccessToken_ShouldGenerateUniqueTokenIds()
    {
        var token1 = new JwtSecurityTokenHandler()
            .ReadJwtToken(_service.GenerateAccessToken(
                1, "patient@test.com", new[] { "Patient" }));

        var token2 = new JwtSecurityTokenHandler()
            .ReadJwtToken(_service.GenerateAccessToken(
                1, "patient@test.com", new[] { "Patient" }));

        Assert.NotEqual(token1.Id, token2.Id);
    }
}