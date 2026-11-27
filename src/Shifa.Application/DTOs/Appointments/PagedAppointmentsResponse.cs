namespace Shifa.Application.DTOs.Appointments;

public class PagedAppointmentsResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public IReadOnlyList<AppointmentResponse> Data { get; set; } = [];
}