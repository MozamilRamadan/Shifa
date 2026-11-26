using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shifa.Application.Interfaces.Permissions;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> CreateRequest(
        int patientId,
        string? reason)
    {
        var userId = GetCurrentUserId();

        var requestId = await _permissionService.CreateRequestAsync(
            patientId, reason, userId);

        return StatusCode(StatusCodes.Status201Created, new
        {
            Id = requestId,
            PatientId = patientId
        });
    }

    [HttpGet("my-requests")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyRequests(
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await _permissionService.GetMyRequestsAsync(
            GetCurrentUserId(), pageNumber, pageSize);

        return Ok(result);
    }

    [HttpPut("{requestId:int}/respond")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> RespondToRequest(
        int requestId,
        bool approve)
    {
        var result = await _permissionService.RespondToRequestAsync(
            requestId, approve, GetCurrentUserId());

        return Ok(result);
    }

    [HttpPut("{requestId:int}/revoke")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> RevokePermission(int requestId)
    {
        var result = await _permissionService.RevokePermissionAsync(
            requestId, GetCurrentUserId());

        return Ok(result);
    }

    [HttpGet("my-approved-patients")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyApprovedPatients(
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result =
            await _permissionService.GetMyApprovedPatientsAsync(
                GetCurrentUserId(), pageNumber, pageSize);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            throw new UnauthorizedAccessException(
                "Invalid user identity.");

        return currentUserId;
    }
}