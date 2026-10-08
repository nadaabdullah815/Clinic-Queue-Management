using ClinicQueue.Models;

namespace ClinicQueue.ViewModels;

public class DoctorDashboardVM
{
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly WorkStartTime { get; set; }
    public TimeOnly WorkEndTime { get; set; }
    public int DailyCapacity { get; set; }
    public int BookedCount { get; set; }
    public bool IsFull => BookedCount >= DailyCapacity;
    public bool IsAcceptingBookings { get; set; }
    public Booking? Current { get; set; }
    public Booking? Next { get; set; }
    public int WaitingCount { get; set; }
    public int DoneCount { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}