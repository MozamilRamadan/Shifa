using Shifa.Application.DTOs.Appointments;
using Shifa.Domain.Entities.Appointments;

namespace Shifa.Application.Interfaces.Appointments;

public interface IAppointmentRepository
{
    Task<int?> GetPatientIdByUserIdAsync(int userId);

    Task<int?> GetDoctorIdByUserIdAsync(int userId);

    Task<bool> DoctorExistsAsync(int doctorId);

    Task<bool> HasOverlappingAppointmentAsync(
        int doctorId,
        DateTime startAt,
        DateTime endAt);

    Task AddAsync(Appointment appointment);

    Task<Appointment?> GetByIdAsync(int appointmentId);

    Task<PagedAppointmentsResponse> GetPatientAppointmentsAsync(
        int patientId,
        int pageNumber,
        int pageSize);

    Task<PagedAppointmentsResponse> GetDoctorAppointmentsAsync(
        int doctorId,
        int pageNumber,
        int pageSize);

    Task SaveChangesAsync();
}