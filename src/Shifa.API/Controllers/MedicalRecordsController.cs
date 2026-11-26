using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.MedicalRecords;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Infrastructure.Persistence;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly ShifaDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public MedicalRecordsController(
        ShifaDbContext context,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    [HttpGet("patient/{patientId:int}")]
    public async Task<IActionResult> GetPatientRecords(
    int patientId,
    int pageNumber = 1,
    int pageSize = 10)
    {
        var authorizationResult =
            await _authorizationService.AuthorizeAsync(
                User,
                patientId,
                "PatientOwnerOrAdmin");

        if (!authorizationResult.Succeeded)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var currentUserId))
                return Forbid();

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUserId);

            if (doctor is null)
                return Forbid();

            var hasPermission = await _context.PermissionRequests
                .AnyAsync(p =>
                    p.PatientId == patientId &&
                    p.DoctorId == doctor.Id &&
                    p.Status == Shifa.Domain.Enums.PermissionStatus.Approved &&
                    (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

            if (!hasPermission)
                return Forbid();

            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("Invalid pagination parameters.");

            pageSize = Math.Min(pageSize, 100);
        }

        var query =
    from record in _context.MedicalRecords.AsNoTracking()
    join doctor in _context.Doctors
        on record.DoctorId equals doctor.Id
    join user in _context.Users
        on doctor.UserId equals user.Id
    where record.PatientId == patientId
    select new
    {
        record.Id,
        record.Title,
        record.Diagnosis,
        record.Description,
        record.Treatment,
        record.Prescription,
        record.RecordDate,
        DoctorName = user.FirstName + " " + user.LastName,
        Files = record.Files.Select(file => new
        {
            file.Id,
            file.FileName,
            file.FileUrl,
            file.FileType,
            file.FileSize,
            file.UploadedAt
        }).ToList()
    };

        var totalCount = await query.CountAsync();

        var records = await query
            .OrderByDescending(r => r.RecordDate)
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
            data = records
        });
    }


    [HttpGet("{recordId:int}")]
    public async Task<IActionResult> GetById(int recordId)
    {
        var record = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.Id == recordId)
            .Select(r => new
            {
                r.Id,
                r.PatientId,
                r.Title,
                r.Diagnosis,
                r.Description,
                r.Treatment,
                r.Prescription,
                r.RecordDate,

                Files = r.Files.Select(file => new
                {
                    file.Id,
                    file.FileName,
                    file.FileUrl,
                    file.FileType,
                    file.FileSize,
                    file.UploadedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (record is null)
            return NotFound();

        var authorizationResult =
            await _authorizationService.AuthorizeAsync(
                User,
                record.PatientId,
                "PatientOwnerOrAdmin");


        if (!authorizationResult.Succeeded)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var currentUserId))
                return Forbid();

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == currentUserId);

            if (doctor is null)
                return Forbid();

            var hasPermission = await _context.PermissionRequests
                .AnyAsync(p =>
                    p.PatientId == record.PatientId &&
                    p.DoctorId == doctor.Id &&
                    p.Status == Shifa.Domain.Enums.PermissionStatus.Approved &&
                    (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

            if (!hasPermission)
                return Forbid();
        }


        return Ok(record);
    }


    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Create(
        CreateMedicalRecordRequest request)
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
            .AnyAsync(p => p.Id == request.PatientId);

        if (!patientExists)
            return NotFound("Patient not found.");

        var hasPermission = await _context.PermissionRequests
        .AnyAsync(p =>
            p.PatientId == request.PatientId &&
            p.DoctorId == doctor.Id &&
            p.Status == Shifa.Domain.Enums.PermissionStatus.Approved &&
            (p.ExpiresAt == null || p.ExpiresAt > DateTime.UtcNow));

            if (!hasPermission)
                return Forbid();

        var record = new MedicalRecord
        {
            PatientId = request.PatientId,
            DoctorId = doctor.Id,
            Title = request.Title,
            Diagnosis = request.Diagnosis,
            Description = request.Description,
            Treatment = request.Treatment,
            Prescription = request.Prescription,
            RecordDate = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { recordId = record.Id },
            new { record.Id });
    }

}