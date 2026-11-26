using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.Interfaces.Patients;
using System.Security.Claims;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuthorizationService _authorizationService;

    public PatientsController(
        IPatientService patientService,
        IAuthorizationService authorizationService)
    {
        _patientService = patientService;
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

        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is null)
            return NotFound();

        return Ok(patient);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return Unauthorized();

        var patient =
            await _patientService.GetMyProfileAsync(currentUserId);

        if (patient is null)
            return NotFound();

        return Ok(patient);
    }
}