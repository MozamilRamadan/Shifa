using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class MedicalRecordConfiguration
    : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Diagnosis)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(5000);

        builder.Property(m => m.Treatment)
            .HasMaxLength(3000);

        builder.Property(m => m.Prescription)
            .HasMaxLength(3000);

        builder.Property(m => m.Status)
            .HasConversion<int>();

        builder.HasIndex(m => m.PatientId);

        builder.HasIndex(m => m.DoctorId);

        builder.HasIndex(m => m.RecordDate);
    }
}