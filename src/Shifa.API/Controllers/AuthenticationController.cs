using Microsoft.AspNetCore.Mvc;
using Shifa.Application.DTOs.Authentication;
using Shifa.Application.Interfaces.Authentication;
using Microsoft.AspNetCore.RateLimiting;
namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(
        IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try { 
            var result = await _authenticationService.RegisterAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _authenticationService.LoginAsync(request);

            if (result is null)
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(423, new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authenticationService.RefreshTokenAsync(request);

        if (result is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request)
    {
        var result = await _authenticationService.LogoutAsync(request);

        if (!result)
            return BadRequest(new { message = "Invalid or revoked refresh token." });

        return Ok(new { message = "Logged out successfully." });
    }

}