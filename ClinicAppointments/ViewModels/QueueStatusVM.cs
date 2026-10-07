namespace ClinicQueue.ViewModels;

public class QueueStatusVM
{
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int CurrentNumber { get; set; }
    public int YourNumber { get; set; }
    public int PatientsBefore { get; set; }
}