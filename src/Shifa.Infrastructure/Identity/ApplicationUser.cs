using Microsoft.AspNetCore.Identity;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Entities.Patients;

namespace Shifa.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }

    public Doctor? Doctor { get; set; }
}