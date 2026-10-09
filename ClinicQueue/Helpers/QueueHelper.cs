using ClinicQueue.Data;
using ClinicQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueue.Helpers;

public interface IQueueHelper
{
    BookingStatus[] ActiveStatuses { get; }
    Task<int> CountedTodayAsync(int doctorId, DateOnly date);
    Task ExpireOldBookingsAsync();
}

public class QueueHelper : IQueueHelper
{
    private readonly AppDbContext _db;

    public BookingStatus[] ActiveStatuses { get; } =
        { BookingStatus.Waiting, BookingStatus.InProgress };

    private static readonly BookingStatus[] Counted =
        { BookingStatus.Waiting, BookingStatus.InProgress, BookingStatus.Done };

    public QueueHelper(AppDbContext db) => _db = db;

    public Task<int> CountedTodayAsync(int doctorId, DateOnly date) =>
        _db.Bookings.CountAsync(b =>
            b.DoctorId == doctorId && b.Date == date && Counted.Contains(b.Status));

    // أي حجز من يوم سابق ما زال بالانتظار أو عند الطبيب يُغلق تلقائياً
    public async Task ExpireOldBookingsAsync()
    {
        var today = QueueClock.Today;
        await _db.Bookings
            .Where(b => b.Date < today &&
                        (b.Status == BookingStatus.Waiting || b.Status == BookingStatus.InProgress))
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.Expired));
    }
}