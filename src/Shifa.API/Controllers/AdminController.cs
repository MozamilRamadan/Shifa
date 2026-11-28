using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.DTOs.Admin;
using Shifa.Application.Interfaces.Admin;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetUsersAsync(
            pageNumber, pageSize);

        return Ok(result);
    }

    [HttpPut("users/{userId:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        int userId,
        [FromBody] UpdateUserStatusRequest request)
    {
        await _adminService.UpdateUserStatusAsync(userId, request);
        return NoContent();
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetDoctorsAsync(
            pageNumber, pageSize);

        return Ok(result);
    }

    [HttpPut("doctors/{doctorId:int}/application-status")]
    public async Task<IActionResult> UpdateDoctorApplicationStatusAsync(
        int doctorId,
        [FromBody] UpdateDoctorVerificationRequest request)
    {
        await _adminService.UpdateDoctorApplicationStatusAsync(
            doctorId, request);

        return NoContent();
    }
}