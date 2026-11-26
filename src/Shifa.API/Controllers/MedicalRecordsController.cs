
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.DTOs.MedicalRecords;
using Shifa.Application.Interfaces.MedicalRecords;
using System.Security.Claims;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IAuthorizationService _authorizationService;

    public MedicalRecordsController(
        IMedicalRecordService medicalRecordService,
        IAuthorizationService authorizationService)
    {
        _medicalRecordService = medicalRecordService;
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

        var currentUserId = GetCurrentUserId();

        var result = await _medicalRecordService.GetPatientRecordsAsync(
            patientId,
            currentUserId,
            authorizationResult.Succeeded,
            pageNumber,
            pageSize);

        return Ok(result);
    }

    [HttpGet("{recordId:int}")]
    public async Task<IActionResult> GetById(int recordId)
    {
        var currentUserId = GetCurrentUserId();

        var patientId =
            await _medicalRecordService.GetPatientIdByRecordIdAsync(recordId);

        if (patientId is null)
            return NotFound();

        var authorizationResult =
            await _authorizationService.AuthorizeAsync(
                User,
                patientId.Value,
                "PatientOwnerOrAdmin");

        var result = await _medicalRecordService.GetByIdAsync(
            recordId,
            currentUserId,
            authorizationResult.Succeeded);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Create(
        CreateMedicalRecordRequest request)
    {
        var currentUserId = GetCurrentUserId();

        var recordId = await _medicalRecordService.CreateAsync(
            request,
            currentUserId);

        return CreatedAtAction(
            nameof(GetById),
            new { recordId },
            new { Id = recordId });
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            throw new UnauthorizedAccessException();

        return currentUserId;
    }

}
