using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shifa.Infrastructure.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;
using Shifa.Application.Interfaces.Authentication;

namespace Shifa.UnitTests.Authentication;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync("missing@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        var jwtService = new Mock<IJwtTokenService>();
        var refreshService = new Mock<IRefreshTokenService>();
        var signInManager = CreateSignInManager(userManager.Object);

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

        var request = new Shifa.Application.DTOs.Authentication.LoginRequest
        {
            Email = "missing@test.com",
            Password = "Password123"
        };

        var result = await service.LoginAsync(request);

        Assert.Null(result);

        signInManager.Verify(
            x => x.CheckPasswordSignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        var user = new ApplicationUser
        {
            Id = 1,
            Email = "inactive@test.com",
            UserName = "inactive@test.com",
            IsActive = false
        };

        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var jwtService = new Mock<IJwtTokenService>();
        var refreshService = new Mock<IRefreshTokenService>();
        var signInManager = CreateSignInManager(userManager.Object);

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

        var request = new Shifa.Application.DTOs.Authentication.LoginRequest
        {
            Email = user.Email,
            Password = "Password123"
        };

        var result = await service.LoginAsync(request);

        Assert.Null(result);

        signInManager.Verify(
            x => x.CheckPasswordSignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
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
