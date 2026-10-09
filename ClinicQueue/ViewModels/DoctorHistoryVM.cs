using ClinicQueue.Models;

namespace ClinicQueue.ViewModels;

public class DoctorHistoryVM
{
    // الفلاتر الحالية
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public BookingStatus? Status { get; set; }
    public string? Search { get; set; }

    // النتائج
    public List<Booking> Items { get; set; } = new();
    public Dictionary<BookingStatus, int> Counts { get; set; } = new();

    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }

    public int PeriodTotal => Counts.Values.Sum();
    public int CountOf(BookingStatus s) => Counts.TryGetValue(s, out var c) ? c : 0;
}