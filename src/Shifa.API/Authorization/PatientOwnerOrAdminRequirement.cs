using Microsoft.AspNetCore.Authorization;

namespace Shifa.API.Authorization;

public sealed class PatientOwnerOrAdminRequirement
    : IAuthorizationRequirement
{
}