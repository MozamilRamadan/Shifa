
using Shifa.Domain.Entities.MedicalRecords;

namespace Shifa.Application.Interfaces.MedicalRecords;

public interface IMedicalFileRepository
{
    Task<MedicalFile?> GetByIdAsync(int fileId);

    Task AddAsync(MedicalFile file);

    Task DeleteAsync(MedicalFile file);

    Task SaveChangesAsync();
}
