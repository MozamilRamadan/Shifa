using Shifa.Application.DTOs.Appointments;

namespace Shifa.Application.Interfaces.Appointments;

public interface IAppointmentService
{
    Task<int> CreateAsync(
        CreateAppointmentRequest request,
        int currentUserId);

    Task<PagedAppointmentsResponse> GetMyPatientAppointmentsAsync(
        int currentUserId,
        int pageNumber,
        int pageSize);

    Task<PagedAppointmentsResponse> GetMyDoctorAppointmentsAsync(
        int currentUserId,
        int pageNumber,
        int pageSize);

    Task<AppointmentResponse> UpdateStatusAsync(
        int appointmentId,
        UpdateAppointmentStatusRequest request,
        int currentUserId);
}