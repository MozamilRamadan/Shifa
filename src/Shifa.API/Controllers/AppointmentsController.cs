
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.DTOs.Appointments;
using Shifa.Application.Interfaces.Appointments;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request)
    {
        var userId = GetCurrentUserId();

        var appointmentId = await _appointmentService.CreateAsync(
            request, userId);

        return StatusCode(201, new { Id = appointmentId });
    }

    [HttpGet("my-patient-appointments")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyPatientAppointments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();

        var result = await _appointmentService
            .GetMyPatientAppointmentsAsync(userId, pageNumber, pageSize);

        return Ok(result);
    }

    [HttpGet("my-doctor-appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyDoctorAppointments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();

        var result = await _appointmentService
            .GetMyDoctorAppointmentsAsync(userId, pageNumber, pageSize);

        return Ok(result);
    }

    [HttpPut("{appointmentId:int}/status")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> UpdateStatus(
        int appointmentId,
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _appointmentService.UpdateStatusAsync(
            appointmentId, request, userId);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user identity.");

        return userId;
    }
}
