
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceRefreshInvalidUserTests
{
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var (service, context, oldToken) = await CreateServiceAsync(null);

        await using (context)
        {
            var result = await service.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = "old-token"
                });

            Assert.Null(result);
            Assert.True(oldToken.IsActive);

            Assert.Equal(
                1,
                await context.RefreshTokens.CountAsync());
        }
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var inactiveUser = new ApplicationUser
        {
            Id = 1,
            Email = "patient@test.com",
            UserName = "patient@test.com",
            IsActive = false
        };

        var (service, context, oldToken) =
            await CreateServiceAsync(inactiveUser);

        await using (context)
        {
            var result = await service.RefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = "old-token"
                });

            Assert.Null(result);
            Assert.True(oldToken.IsActive);

            Assert.Equal(
                1,
                await context.RefreshTokens.CountAsync());
        }
    }

    private static async Task<(
        AuthenticationService Service,
        ShifaDbContext Context,
        RefreshToken OldToken)> CreateServiceAsync(
            ApplicationUser? user)
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        var refreshService = new Mock<IRefreshTokenService>();

        refreshService
            .Setup(x => x.HashToken("old-token"))
            .Returns("old-hash");

        var jwtService = new Mock<IJwtTokenService>();

        var options = Options.Create(new JwtOptions
        {
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

        var dbOptions = new DbContextOptionsBuilder<ShifaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ShifaDbContext(dbOptions);

        var oldToken = new RefreshToken
        {
            UserId = 1,
            TokenHash = "old-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        context.RefreshTokens.Add(oldToken);
        await context.SaveChangesAsync();

        var service = new AuthenticationService(
            userManager.Object,
            jwtService.Object,
            refreshService.Object,
            options,
            context,
            signInManager.Object);

        return (service, context, oldToken);
    }
}
