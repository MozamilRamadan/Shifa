
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

public class AuthenticationServiceRegisterSuccessTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnTokens_WhenRequestIsValid()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync("patient@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

        userManager
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password123!"))
            .Callback<ApplicationUser, string>((createdUser, _) =>
            {
                createdUser.Id = 1;
            })
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "Patient"))
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.GetRolesAsync(
                It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Patient" });

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
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        refreshService
            .Setup(x => x.HashToken("new-refresh-token"))
            .Returns("new-refresh-hash");

        var jwtService = new Mock<IJwtTokenService>();

        jwtService
            .Setup(x => x.GenerateAccessToken(
                1,
                "patient@test.com",
                It.Is<IEnumerable<string>>(roles =>
                    roles.Contains("Patient"))))
            .Returns("new-access-token");

        var options = Options.Create(new JwtOptions
        {
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

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

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Email = "patient@test.com",
            FirstName = "Test",
            LastName = "Patient",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("patient@test.com", result.Email);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Contains("Patient", result.Roles);

        userManager.Verify(
            x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                "Password123!"),
            Times.Once);

        userManager.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "Patient"),
            Times.Once);

        var storedToken = await context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.TokenHash == "new-refresh-hash");

        Assert.NotNull(storedToken);
        Assert.Equal(1, storedToken.UserId);
        Assert.True(storedToken.IsActive);
    }
}
