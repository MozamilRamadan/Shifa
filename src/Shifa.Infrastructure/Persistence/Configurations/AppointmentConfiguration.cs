using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.Appointments;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration
    : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.Status)
            .HasConversion<int>();

        builder.HasIndex(a => new { a.DoctorId, a.StartAt });

        builder.HasIndex(a => new { a.PatientId, a.StartAt });

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_Appointment_ValidTime",
                "\"EndAt\" > \"StartAt\""));
    }
}