using ClinicAppointments.Data;
using ClinicAppointments.Models;
using ClinicAppointments.ViewModels;
using Microsoft.EntityFrameworkCore;
using ClinicAppointments.Services.Interfaces;

namespace ClinicAppointments.Services;

public record BookResult(bool Success, string Message);

public class QueueService
{
    private readonly AppDbContext _db;
    private readonly IDoctorProvider _doctors;

    private static readonly BookingStatus[] Active = { BookingStatus.Waiting, BookingStatus.InProgress };

    public QueueService(AppDbContext db, IDoctorProvider doctors)
    {
        _db = db;
        _doctors = doctors;
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

    // ================= المريض =================

    public async Task<BookResult> BookAsync(int userId)
    {
        var doctor = await _doctors.GetAsync();
        if (!doctor.IsAcceptingBookings)
            return new(false, "الحجز مغلق حالياً");

        var today = Today;

        var hasActive = await _db.Bookings.AnyAsync(b =>
            b.UserId == userId && b.Date == today && Active.Contains(b.Status));
        if (hasActive)
            return new(false, "لديك دور فعّال اليوم");

        const int maxBookingsPerDay = 5;
        var todayCount = await _db.Bookings.CountAsync(b => b.UserId == userId && b.Date == today);
        if (todayCount >= maxBookingsPerDay)
            return new(false, "لقد بلغت الحد الأقصى للحجوزات اليوم");

        // نعيد المحاولة إذا مريض تاني أخذ نفس الرقم بنفس اللحظة (Unique Index)
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var last = await _db.Bookings
                .Where(b => b.DoctorId == doctor.Id && b.Date == today)
                .MaxAsync(b => (int?)b.QueueNumber) ?? 0;

            var booking = new Booking
            {
                UserId = userId,
                DoctorId = doctor.Id,
                Date = today,
                QueueNumber = last + 1
            };
            _db.Bookings.Add(booking);

            try
            {
                await _db.SaveChangesAsync();
                return new(true, $"تم حجز دورك بنجاح. رقمك {booking.QueueNumber}");
            }
            catch (DbUpdateException)
            {
                _db.Entry(booking).State = EntityState.Detached;
            }
        }

        return new(false, "الخدمة مشغولة حاليا ، يرجى المحاولة مرى أخرى");
    }

    public async Task<bool> CancelAsync(int bookingId, int userId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.Id == bookingId && b.UserId == userId && b.Status == BookingStatus.Waiting);
        if (booking == null) return false;

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<QueueStatusVM> GetStatusAsync(int userId)
    {
        var doctor = await _doctors.GetAsync();
        var today = Today;
        var todays = _db.Bookings.AsNoTracking().Where(b => b.DoctorId == doctor.Id && b.Date == today);

        var current = await todays
            .Where(b => b.Status == BookingStatus.InProgress)
            .Select(b => (int?)b.QueueNumber)
            .FirstOrDefaultAsync() ?? 0;

        var mine = await todays
            .Where(b => b.UserId == userId && Active.Contains(b.Status))
            .OrderBy(b => b.QueueNumber)
            .FirstOrDefaultAsync();

        var before = 0;
        if (mine != null)
        {
            var myNumber = mine.QueueNumber;
            before = await todays.CountAsync(b =>
                b.Status == BookingStatus.Waiting && b.QueueNumber < myNumber);
        }

        return new QueueStatusVM
        {
            DoctorName = doctor.Name,
            Date = today,
            CurrentNumber = current,
            YourNumber = mine?.QueueNumber ?? 0,
            PatientsBefore = before,
            IsYourTurn = mine?.Status == BookingStatus.InProgress,
            CanCancel = mine?.Status == BookingStatus.Waiting,
            BookingId = mine?.Id
        };
    }

    public async Task<BookPageVM> GetBookPageAsync(int userId)
    {
        var doctor = await _doctors.GetAsync();
        var today = Today;
        var todays = _db.Bookings.AsNoTracking().Where(b => b.DoctorId == doctor.Id && b.Date == today);

        return new BookPageVM
        {
            Doctor = doctor,
            CurrentNumber = await todays
                .Where(b => b.Status == BookingStatus.InProgress)
                .Select(b => (int?)b.QueueNumber)
                .FirstOrDefaultAsync() ?? 0,
            WaitingCount = await todays.CountAsync(b => b.Status == BookingStatus.Waiting),
            HasActive = await todays.AnyAsync(b => b.UserId == userId && Active.Contains(b.Status))
        };
    }

    public Task<List<Booking>> GetHistoryAsync(int userId) =>
        _db.Bookings.AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Date)
            .ThenByDescending(b => b.QueueNumber)
            .ToListAsync();

    // ================= الطبيب (منستخدمها بالرد الجاي) =================

    public async Task<List<Booking>> GetTodayAsync()
    {
        var doctor = await _doctors.GetAsync();
        var today = Today;
        return await _db.Bookings.AsNoTracking()
            .Include(b => b.User)
            .Where(b => b.DoctorId == doctor.Id && b.Date == today)
            .OrderBy(b => b.QueueNumber)
            .ToListAsync();
    }

    // يقفل الحالي (Done) ويفتح التالي (InProgress) بعملية وحدة
    public async Task<bool> NextAsync()
    {
        var doctor = await _doctors.GetAsync();
        var today = Today;

        await using var tx = await _db.Database.BeginTransactionAsync();

        var todays = _db.Bookings.Where(b => b.DoctorId == doctor.Id && b.Date == today);

        var current = await todays.FirstOrDefaultAsync(b => b.Status == BookingStatus.InProgress);
        if (current != null) current.Status = BookingStatus.Done;

        var next = await todays
            .Where(b => b.Status == BookingStatus.Waiting)
            .OrderBy(b => b.QueueNumber)
            .FirstOrDefaultAsync();
        if (next != null) next.Status = BookingStatus.InProgress;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return next != null;   // false = خلصت القائمة
    }

    public Task<bool> SkipAsync(int bookingId) => ChangeStatusAsync(bookingId, BookingStatus.Skipped);
    public Task<bool> CancelByDoctorAsync(int bookingId) => ChangeStatusAsync(bookingId, BookingStatus.Cancelled);

    private async Task<bool> ChangeStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.Id == bookingId && Active.Contains(b.Status));
        if (booking == null) return false;

        booking.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }
}