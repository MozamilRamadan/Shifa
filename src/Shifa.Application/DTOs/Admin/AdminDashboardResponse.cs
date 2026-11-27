
namespace Shifa.Application.DTOs.Admin;

public class AdminDashboardResponse
{
    public int TotalUsers { get; set; }

    public int TotalPatients { get; set; }

    public int TotalDoctors { get; set; }

    public int VerifiedDoctors { get; set; }

    public int PendingDoctors { get; set; }

    public int TotalAppointments { get; set; }

    public int PendingAppointments { get; set; }

    public int TotalMedicalRecords { get; set; }
}
