using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.Permissions;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class PermissionRequestConfiguration
    : IEntityTypeConfiguration<PermissionRequest>
{
    public void Configure(EntityTypeBuilder<PermissionRequest> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Reason)
            .HasMaxLength(1000);

        builder.Property(p => p.Status)
            .HasConversion<int>();

        builder.HasIndex(p => new { p.PatientId, p.DoctorId });

        builder.HasIndex(p => p.Status);
    }
}