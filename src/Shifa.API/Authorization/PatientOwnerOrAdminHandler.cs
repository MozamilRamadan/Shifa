using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Shifa.Infrastructure.Persistence;

namespace Shifa.API.Authorization;

public class PatientOwnerOrAdminHandler
    : AuthorizationHandler<PatientOwnerOrAdminRequirement, int>
{
    private readonly ShifaDbContext _context;

    public PatientOwnerOrAdminHandler(ShifaDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PatientOwnerOrAdminRequirement requirement,
        int patientId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var currentUserId))
            return;

        var isOwner = await _context.Patients
            .AnyAsync(p =>
                p.Id == patientId &&
                p.UserId == currentUserId);

        if (isOwner)
            context.Succeed(requirement);
    }
}