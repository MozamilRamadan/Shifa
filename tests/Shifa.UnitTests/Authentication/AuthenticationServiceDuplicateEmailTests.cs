using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceDuplicateEmailTests
{
    [Fact]
    public async Task RegisterAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        var existingUser = new ApplicationUser
        {
            Id = 1,
            Email = "existing@test.com",
            UserName = "existing@test.com"
        };

        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync(existingUser.Email!))
            .ReturnsAsync(existingUser);

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
            Email = existingUser.Email!,
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegisterAsync(request));

        Assert.Equal("Email is already registered.", exception.Message);

        userManager.Verify(
            x => x.CreateAsync(
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