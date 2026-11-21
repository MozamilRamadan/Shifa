using Shifa.Application.DTOs.Authentication;

namespace Shifa.Application.Interfaces.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> LoginAsync(
        LoginRequest request);

    Task<AuthenticationResponse> RegisterAsync(
        RegisterRequest request);

    Task<AuthenticationResponse?> RefreshTokenAsync(
    RefreshTokenRequest request);

    Task<bool> LogoutAsync(LogoutRequest request);
}