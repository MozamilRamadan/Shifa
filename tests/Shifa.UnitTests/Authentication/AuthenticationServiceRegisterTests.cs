using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceRegisterTests
{
    [Fact]
    public async Task RegisterAsync_ShouldThrowArgumentException_WhenPasswordsDoNotMatch()
    {
        var userManager = CreateUserManager();
        var signInManager = CreateSignInManager(userManager.Object);

        var jwtService = new Mock<IJwtTokenService>();
        var refreshService = new Mock<IRefreshTokenService>();

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

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Password = "Password123",
            ConfirmPassword = "Different123"
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.RegisterAsync(request));

        Assert.Equal("Passwords do not match.", exception.Message);

        userManager.Verify(
            x => x.FindByEmailAsync(It.IsAny<string>()),
            Times.Never);

        userManager.Verify(
            x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
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