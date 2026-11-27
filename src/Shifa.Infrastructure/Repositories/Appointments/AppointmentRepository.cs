using Microsoft.EntityFrameworkCore;
using Shifa.Application.DTOs.Appointments;
using Shifa.Application.Interfaces.Appointments;
using Shifa.Domain.Entities.Appointments;
using Shifa.Domain.Enums;
using Shifa.Infrastructure.Persistence;

namespace Shifa.Infrastructure.Repositories.Appointments;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ShifaDbContext _context;

    public AppointmentRepository(ShifaDbContext context)
    {
        _context = context;
    }

    public async Task<int?> GetPatientIdByUserIdAsync(int userId)
    {
        return await _context.Patients
            .Where(p => p.UserId == userId)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetDoctorIdByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Where(d => d.UserId == userId)
            .Select(d => (int?)d.Id)
            .FirstOrDefaultAsync();
    }

    public Task<bool> DoctorExistsAsync(int doctorId)
    {
        return _context.Doctors.AnyAsync(d => d.Id == doctorId);
    }

    public Task<bool> HasOverlappingAppointmentAsync(
        int doctorId,
        DateTime startAt,
        DateTime endAt)
    {
        return _context.Appointments.AnyAsync(a =>
            a.DoctorId == doctorId &&
            a.Status != AppointmentStatus.Cancelled &&
            startAt < a.EndAt &&
            endAt > a.StartAt);
    }

    public async Task AddAsync(Appointment appointment)
    {
        await _context.Appointments.AddAsync(appointment);
    }

    public Task<Appointment?> GetByIdAsync(int appointmentId)
    {
        return _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
    }

    public async Task<PagedAppointmentsResponse>
        GetPatientAppointmentsAsync(
            int patientId,
            int pageNumber,
            int pageSize)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patientId);

        return await BuildPagedResponseAsync(
            query, pageNumber, pageSize);
    }

    public async Task<PagedAppointmentsResponse>
        GetDoctorAppointmentsAsync(
            int doctorId,
            int pageNumber,
            int pageSize)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId);

        return await BuildPagedResponseAsync(
            query, pageNumber, pageSize);
    }

    private static async Task<PagedAppointmentsResponse>
        BuildPagedResponseAsync(
            IQueryable<Appointment> query,
            int pageNumber,
            int pageSize)
    {
        var totalCount = await query.CountAsync();

        var appointments = await query
            .OrderBy(a => a.StartAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse
            {
                Id = a.Id,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                StartAt = a.StartAt,
                EndAt = a.EndAt,
                Reason = a.Reason,
                Notes = a.Notes,
                Status = a.Status.ToString()
            })
            .ToListAsync();

        return new PagedAppointmentsResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize),
            Data = appointments
        };
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}