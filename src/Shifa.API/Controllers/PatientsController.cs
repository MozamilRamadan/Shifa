using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shifa.Infrastructure.Persistence;
using System.Security.Claims;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly ShifaDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public PatientsController(
        ShifaDbContext context,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }
    [HttpGet("{patientId:int}")]
    public async Task<IActionResult> GetById(int patientId)
    {
        var authorizationResult =
            await _authorizationService.AuthorizeAsync(
                User,
                patientId,
                "PatientOwnerOrAdmin");

        if (!authorizationResult.Succeeded)
            return Forbid();

        var patient = await _context.Patients
            .AsNoTracking()
            .Where(p => p.Id == patientId)
            .Select(p => new
            {
                p.Id,
                p.DateOfBirth,
                p.Gender,
                p.BloodType,
                p.Address,
                p.EmergencyContactName,
                p.EmergencyContactPhone
            })
            .FirstOrDefaultAsync();

        if (patient is null)
            return NotFound();

        return Ok(patient);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var patient = await _context.Patients
            .AsNoTracking()
            .Where(p => p.UserId == currentUserId)
            .Select(p => new
            {
                p.Id,
                p.DateOfBirth,
                p.Gender,
                p.BloodType,
                p.Address,
                p.EmergencyContactName,
                p.EmergencyContactPhone
            })
            .FirstOrDefaultAsync();

        if (patient is null)
            return NotFound();

        return Ok(patient);
    }

}