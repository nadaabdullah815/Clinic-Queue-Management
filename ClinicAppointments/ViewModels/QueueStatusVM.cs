namespace ClinicAppointments.ViewModels;

public class QueueStatusVM
{
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int CurrentNumber { get; set; }
    public int YourNumber { get; set; }
    public int PatientsBefore { get; set; }
    public bool IsYourTurn { get; set; }
    public bool CanCancel { get; set; }
    public int? BookingId { get; set; }
}