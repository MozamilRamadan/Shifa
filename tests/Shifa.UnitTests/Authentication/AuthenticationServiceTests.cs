using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        // Arrange
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        userManager
            .Setup(x => x.FindByEmailAsync("missing@test.com"))
            .ReturnsAsync((ApplicationUser?)null);

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
            context);

        var request = new Shifa.Application.DTOs.Authentication.LoginRequest
        {
            Email = "missing@test.com",
            Password = "Password123"
        };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        // Arrange
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
            context);

        var request = new Shifa.Application.DTOs.Authentication.LoginRequest
        {
            Email = user.Email,
            Password = "Password123"
        };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.Null(result);

        userManager.Verify(
            x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }



}