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
    public async Task<IActionResult> GetMyRequests(
    int pageNumber = 1,
    int pageSize = 10)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        if (patient is null)
            return NotFound("Patient profile not found.");

        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Invalid pagination parameters.");

        pageSize = Math.Min(pageSize, 100);

        var query = _context.PermissionRequests
            .AsNoTracking()
            .Where(p => p.PatientId == patient.Id);

        var totalCount = await query.CountAsync();

        var requests = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            data = requests
        });
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

    [HttpPut("{requestId:int}/revoke")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> RevokePermission(int requestId)
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

        if (request.Status != PermissionStatus.Approved)
            return Conflict("Only approved permissions can be revoked.");

        request.Status = PermissionStatus.Revoked;
        request.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            request.Id,
            request.Status,
            request.RespondedAt
        });
    }

    [HttpGet("my-approved-patients")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyApprovedPatients(
    int pageNumber = 1,
    int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("Invalid pagination parameters.");

        pageSize = Math.Min(pageSize, 100);

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.UserId == currentUserId);

        if (doctor is null)
            return Forbid();

        var query =
            from permission in _context.PermissionRequests.AsNoTracking()
            join patient in _context.Patients
                on permission.PatientId equals patient.Id
            join user in _context.Users
                on patient.UserId equals user.Id
            where permission.DoctorId == doctor.Id
                && permission.Status == PermissionStatus.Approved
                && (permission.ExpiresAt == null ||
                    permission.ExpiresAt > DateTime.UtcNow)
            select new
            {
                PatientId = patient.Id,
                FullName = user.FirstName + " " + user.LastName,
                permission.Status,
                permission.ExpiresAt
            };

        var totalCount = await query.CountAsync();

        var patients = await query
            .OrderBy(p => p.PatientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            data = patients
        });
    }


}