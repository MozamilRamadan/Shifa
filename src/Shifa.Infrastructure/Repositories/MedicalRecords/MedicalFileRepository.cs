
using Microsoft.EntityFrameworkCore;
using Shifa.Application.Interfaces.MedicalRecords;
using Shifa.Domain.Entities.MedicalRecords;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.MedicalRecords;

public class MedicalFileRepository : IMedicalFileRepository
{
    private readonly ShifaDbContext _context;

    public MedicalFileRepository(ShifaDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalFile?> GetByIdAsync(int fileId)
    {
        return await _context.MedicalFiles
            .FirstOrDefaultAsync(file => file.Id == fileId);
    }

    public async Task AddAsync(MedicalFile file)
    {
        await _context.MedicalFiles.AddAsync(file);
    }

    public Task DeleteAsync(MedicalFile file)
    {
        _context.MedicalFiles.Remove(file);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
