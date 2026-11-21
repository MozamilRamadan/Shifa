using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.Patients;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Gender)
            .HasMaxLength(20);

        builder.Property(p => p.BloodType)
            .HasMaxLength(5);

        builder.Property(p => p.Address)
            .HasMaxLength(500);

        builder.Property(p => p.EmergencyContactName)
            .HasMaxLength(150);

        builder.Property(p => p.EmergencyContactPhone)
            .HasMaxLength(20);

        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}