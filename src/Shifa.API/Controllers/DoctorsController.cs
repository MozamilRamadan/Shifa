using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.DTOs.Doctors;
using Shifa.Application.Interfaces.Doctors;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var profile = await _doctorService.GetMyProfileAsync(userId);

        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPost("profile")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateDoctorProfileRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var profile = await _doctorService.CreateProfileAsync(userId, request);

        return CreatedAtAction(
            nameof(GetMyProfile),
            profile);
    }
}