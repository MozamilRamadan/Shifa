using Shifa.Application.DTOs.Appointments;
using Shifa.Application.Exceptions;
using Shifa.Application.Interfaces.Appointments;
using Shifa.Domain.Entities.Appointments;
using Shifa.Domain.Enums;

namespace Shifa.Application.Services.Appointments;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;

    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateAsync(
        CreateAppointmentRequest request,
        int currentUserId)
    {
        var patientId =
            await _repository.GetPatientIdByUserIdAsync(currentUserId);

        if (patientId is null)
            throw new ForbiddenException(
                "Only registered patients can book appointments.");

        if (request.StartAt <= DateTime.UtcNow)
            throw new ArgumentException(
                "Appointment must be scheduled in the future.");

        if (request.EndAt <= request.StartAt)
            throw new ArgumentException(
                "Appointment end time must be after start time.");

        if (request.StartAt.Kind != DateTimeKind.Utc ||
            request.EndAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException(
                "Appointment times must be provided in UTC.");

        if (!await _repository.DoctorExistsAsync(request.DoctorId))
            throw new NotFoundException("Doctor not found.");

        var hasOverlap =
            await _repository.HasOverlappingAppointmentAsync(
                request.DoctorId,
                request.StartAt,
                request.EndAt);

        if (hasOverlap)
            throw new ConflictException(
                "Doctor already has an appointment during this time.");

        var appointment = new Appointment
        {
            PatientId = patientId.Value,
            DoctorId = request.DoctorId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Reason = request.Reason,
            Status = AppointmentStatus.Pending
        };

        await _repository.AddAsync(appointment);
        await _repository.SaveChangesAsync();

        return appointment.Id;
    }

    public async Task<PagedAppointmentsResponse>
        GetMyPatientAppointmentsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        var patientId =
            await _repository.GetPatientIdByUserIdAsync(currentUserId);

        if (patientId is null)
            throw new NotFoundException("Patient profile not found.");

        return await _repository.GetPatientAppointmentsAsync(
            patientId.Value, pageNumber, pageSize);
    }

    public async Task<PagedAppointmentsResponse>
        GetMyDoctorAppointmentsAsync(
            int currentUserId,
            int pageNumber,
            int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        var doctorId =
            await _repository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null)
            throw new NotFoundException("Doctor profile not found.");

        return await _repository.GetDoctorAppointmentsAsync(
            doctorId.Value, pageNumber, pageSize);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(
        int appointmentId,
        UpdateAppointmentStatusRequest request,
        int currentUserId)
    {
        var appointment =
            await _repository.GetByIdAsync(appointmentId);

        if (appointment is null)
            throw new NotFoundException("Appointment not found.");

        var doctorId =
            await _repository.GetDoctorIdByUserIdAsync(currentUserId);

        if (doctorId is null || appointment.DoctorId != doctorId.Value)
            throw new ForbiddenException(
                "You cannot update this appointment.");

        if (!Enum.TryParse<AppointmentStatus>(
                request.Status, true, out var newStatus) ||
            !Enum.IsDefined(newStatus))
            throw new ArgumentException(
                "Invalid appointment status.");

        if (appointment.Status == AppointmentStatus.Cancelled ||
            appointment.Status == AppointmentStatus.Completed ||
            appointment.Status == AppointmentStatus.NoShow)
            throw new ConflictException(
                "This appointment can no longer be updated.");

        appointment.Status = newStatus;
        appointment.Notes = request.Notes;

        await _repository.SaveChangesAsync();

        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            StartAt = appointment.StartAt,
            EndAt = appointment.EndAt,
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            Status = appointment.Status.ToString()
        };
    }

    private static void ValidatePagination(
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1)
            throw new ArgumentException(
                "Invalid pagination parameters.");

        if (pageSize > 100)
            throw new ArgumentException(
                "Page size cannot exceed 100.");
    }
}