
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shifa.Domain.Entities.Administration;
using Shifa.Domain.Entities.Appointments;
using Shifa.Domain.Entities.Doctors;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Domain.Entities.Patients;
using Shifa.Domain.Entities.Permissions;
using Shifa.Infrastructure.Identity;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Persistence;

public class ShifaDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ShifaDbContext(DbContextOptions<ShifaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalFile> MedicalFiles => Set<MedicalFile>();
    public DbSet<PermissionRequest> PermissionRequests => Set<PermissionRequest>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ShifaDbContext).Assembly);

        // ApplicationUser - Patient
        builder.Entity<Patient>()
            .HasOne<ApplicationUser>()
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ApplicationUser - Doctor
        builder.Entity<Doctor>()
            .HasOne<ApplicationUser>()
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // RefreshToken - ApplicationUser
        builder.Entity<RefreshToken>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
            .IsUnique();

        // MedicalRecord - Patient
        builder.Entity<MedicalRecord>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalRecord - Doctor
        builder.Entity<MedicalRecord>()
            .HasOne(m => m.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedicalFile - MedicalRecord
        builder.Entity<MedicalFile>()
            .HasOne(f => f.MedicalRecord)
            .WithMany(m => m.Files)
            .HasForeignKey(f => f.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // PermissionRequest relationships
        builder.Entity<PermissionRequest>()
            .HasOne(p => p.Patient)
            .WithMany(p => p.PermissionRequests)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PermissionRequest>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.PermissionRequests)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment relationships
        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filters
        builder.Entity<Patient>()
            .HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<Doctor>()
            .HasQueryFilter(d => !d.IsDeleted);

        builder.Entity<MedicalRecord>()
            .HasQueryFilter(m => !m.IsDeleted);

        builder.Entity<PermissionRequest>()
            .HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<Appointment>()
            .HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<MedicalFile>()
            .HasQueryFilter(f => !f.MedicalRecord.IsDeleted);
    }
}
