using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Enums;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Specialization)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Qualification)
            .HasMaxLength(200);

        builder.Property(d => d.ClinicAddress)
            .HasMaxLength(500);

        builder.HasIndex(d => d.UserId)
            .IsUnique();

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique();

        builder.Property(d => d.ApplicationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(DoctorApplicationStatus.Pending);
    }
}