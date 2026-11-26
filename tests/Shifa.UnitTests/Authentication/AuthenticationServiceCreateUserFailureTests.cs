using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceCreateUserFailureTests
{
    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenUserCreationFails()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        userManager
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password123"))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password is too weak." }));

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
            ConfirmPassword = "Password123"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterAsync(request));

        Assert.Equal("Password is too weak.", exception.Message);

        userManager.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
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