using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly ShifaDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtOptions> jwtOptions,
        ShifaDbContext context,
        SignInManager<ApplicationUser> signInManager
        )
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions.Value;
        _context = context;
        _signInManager = signInManager;
    }

    public async Task<AuthenticationResponse?> LoginAsync(
        LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return null;

        if (!user.IsActive)
            return null;

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

            throw new UnauthorizedAccessException(
                $"Account is temporarily locked until {lockoutEnd:O}.");
        }

        if (!signInResult.Succeeded)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            roles);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenExpirationMinutes),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    public async Task<AuthenticationResponse> RegisterAsync(
        RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("Email is already registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }
        var roleResult = await _userManager.AddToRoleAsync(user, "Patient");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            roles);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays)
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenExpirationMinutes),
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(
        RefreshTokenRequest request)
    {
        var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken is null || !storedToken.IsActive)
            return null;

        var user = await _userManager.FindByIdAsync(
            storedToken.UserId.ToString());

        if (user is null || !user.IsActive)
            return null;

        // Revoke the old refresh token to prevent reuse.
        storedToken.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email!,
            roles);

        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _refreshTokenService.HashToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtOptions.RefreshTokenExpirationDays)
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _context.SaveChangesAsync();

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            AccessToken = accessToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtOptions.AccessTokenExpirationMinutes),
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt
        };
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        var tokenHash = _refreshTokenService.HashToken(
            request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken is null || !storedToken.IsActive)
            return false;

        storedToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

}