using ClinicQueue.Data;
using ClinicQueue.Models;
using ClinicQueue.ViewModels;
using Microsoft.EntityFrameworkCore;
using ClinicQueue.Services.Interfaces;
using ClinicQueue.Helper;
namespace ClinicQueue.Services;


public class DoctorQueueService :IDoctorQueueService
{
    private readonly AppDbContext _db;
    private readonly IDoctorProvider _doctors;
    private readonly IPatientHelperFunctions _helper;
    public DoctorQueueService(AppDbContext db, IDoctorProvider doctors, IPatientHelperFunctions helper)
    {
        _db = db;
        _doctors = doctors;
        _helper = helper;
    } 

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Now);

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

    public async Task SetAcceptingBookingsAsync(bool open)
    {
        var doctor = await _db.Doctors.FirstAsync();
        doctor.IsAcceptingBookings = open;
        await _db.SaveChangesAsync();
    }

     // إعادة مريض متخطّى إلى قائمة الانتظار (برقمه القديم، فيُستدعى كأول واحد)
    public async Task<bool> RequeueAsync(int bookingId)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.Id == bookingId && b.Status == BookingStatus.Skipped);
        if (booking == null) return false;
    
        booking.Status = BookingStatus.Waiting;
        await _db.SaveChangesAsync();
        return true;
   }

    public async Task UpdateWorkingHoursAsync(TimeOnly start, TimeOnly end)
    {
        var doctor = await _db.Doctors.FirstAsync();
        doctor.WorkStartTime = start;
        doctor.WorkEndTime = end;
        await _db.SaveChangesAsync();
    }

      public async Task UpdateCapacityAsync(int capacity)
    {
        var doctor = await _db.Doctors.FirstAsync();
        doctor.DailyCapacity = capacity;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsFullAsync()
    {
        var doctor = await _doctors.GetAsync();
        return await _helper.CountedTodayAsync(doctor.Id, Today) >= doctor.DailyCapacity;
    }


      private async Task<bool> ChangeStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.Id == bookingId && _helper.ActiveStatuses.Contains(b.Status));
        if (booking == null) return false;

        booking.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }
    


}
