using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Infrastructure.Persistence.Configurations;

public class MedicalFileConfiguration
    : IEntityTypeConfiguration<MedicalFile>
{
    public void Configure(EntityTypeBuilder<MedicalFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.FileUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(f => f.FileType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.StorageIdentifier)
            .HasMaxLength(500);

        builder.HasIndex(f => f.MedicalRecordId);
    }
}