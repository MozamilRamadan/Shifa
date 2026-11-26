using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Application.DTOs.Authentication;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceLogoutTests
{
    [Fact]
    public async Task LogoutAsync_ShouldReturnFalse_WhenTokenDoesNotExist()
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager.Object);

        var refreshService = new Mock<IRefreshTokenService>();
        refreshService
            .Setup(x => x.HashToken("unknown-token"))
            .Returns("unknown-hash");

        var jwtService = new Mock<IJwtTokenService>();

        var options = Options.Create(new JwtOptions());

        var dbOptions = new DbContextOptionsBuilder<ShifaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ShifaDbContext(dbOptions);

        var service = new AuthenticationService(
            userManager.Object,
            jwtService.Object,
            refreshService.Object,
            options,
            context,
            signInManager.Object);

        var result = await service.LogoutAsync(new LogoutRequest
        {
            RefreshToken = "unknown-token"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeToken_WhenTokenIsActive()
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager.Object);

        var refreshService = new Mock<IRefreshTokenService>();
        refreshService
            .Setup(x => x.HashToken("valid-token"))
            .Returns("valid-hash");

        var jwtService = new Mock<IJwtTokenService>();

        var options = Options.Create(new JwtOptions());

        var dbOptions = new DbContextOptionsBuilder<ShifaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ShifaDbContext(dbOptions);

        var storedToken = new RefreshToken
        {
            UserId = 1,
            TokenHash = "valid-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        context.RefreshTokens.Add(storedToken);
        await context.SaveChangesAsync();

        var service = new AuthenticationService(
            userManager.Object,
            jwtService.Object,
            refreshService.Object,
            options,
            context,
            signInManager.Object);

        var result = await service.LogoutAsync(new LogoutRequest
        {
            RefreshToken = "valid-token"
        });

        Assert.True(result);
        Assert.NotNull(storedToken.RevokedAt);
        Assert.False(storedToken.IsActive);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManager(
        UserManager<ApplicationUser> userManager)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
    }
}