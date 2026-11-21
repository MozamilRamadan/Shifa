using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.Administration;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Property(a => a.Details)
            .HasMaxLength(4000);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.HasIndex(a => a.Timestamp);

        builder.HasIndex(a => a.UserId);
    }
}