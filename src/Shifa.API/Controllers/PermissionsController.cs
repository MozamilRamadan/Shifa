using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shifa.Infrastructure.Persistence;
using Shifa.Domain.Entities.Permissions;
using Shifa.Domain.Enums;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly ShifaDbContext _context;

    public PermissionsController(ShifaDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CreateRequest(
    int patientId,
    string? reason)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == currentUserId);

        if (doctor is null)
            return Forbid();

        var patientExists = await _context.Patients
            .AnyAsync(p => p.Id == patientId);

        if (!patientExists)
            return NotFound("Patient not found.");

        var existingRequest = await _context.PermissionRequests
            .AnyAsync(p =>
                p.PatientId == patientId &&
                p.DoctorId == doctor.Id &&
                p.Status == PermissionStatus.Pending);

        if (existingRequest)
            return Conflict("A pending request already exists.");

        var request = new PermissionRequest
        {
            PatientId = patientId,
            DoctorId = doctor.Id,
            Reason = reason,
            Status = PermissionStatus.Pending
        };

        _context.PermissionRequests.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(CreateRequest),
            new { id = request.Id },
            new { request.Id, request.PatientId, request.DoctorId, request.Status });
    }

    [HttpGet("my-requests")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        if (patient is null)
            return NotFound("Patient profile not found.");

        var requests = await _context.PermissionRequests
            .AsNoTracking()
            .Where(p => p.PatientId == patient.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.DoctorId,
                p.Reason,
                p.Status,
                p.CreatedAt,
                p.ExpiresAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPut("{requestId:int}/respond")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> RespondToRequest(
    int requestId,
    bool approve)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        if (patient is null)
            return NotFound("Patient profile not found.");

        var request = await _context.PermissionRequests
            .FirstOrDefaultAsync(p =>
                p.Id == requestId &&
                p.PatientId == patient.Id);

        if (request is null)
            return NotFound("Permission request not found.");

        if (request.Status != PermissionStatus.Pending)
            return Conflict("Request has already been processed.");

        request.Status = approve
            ? PermissionStatus.Approved
            : PermissionStatus.Rejected;

        request.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            request.Id,
            request.Status,
            request.RespondedAt
        });
    }
}