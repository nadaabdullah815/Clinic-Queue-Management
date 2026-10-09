using ClinicQueue.Data;
using ClinicQueue.Models;
using ClinicQueue.ViewModels;
using Microsoft.EntityFrameworkCore;
using ClinicQueue.Services.Interfaces;
using ClinicQueue.Helpers;
namespace ClinicQueue.Services;


public class DoctorQueueService :IDoctorQueueService
{
    private readonly AppDbContext _db;
    private readonly IDoctorProvider _doctors;
    private readonly IQueueHelper _helper;
    public DoctorQueueService(AppDbContext db, IDoctorProvider doctors, IQueueHelper helper)
    {
        _db = db;
        _doctors = doctors;
        _helper = helper;
    } 

  private static DateOnly Today => QueueClock.Today;

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
        var today = Today;
        var booking = await _db.Bookings.FirstOrDefaultAsync(b =>
            b.Id == bookingId && b.Date == today && b.Status == BookingStatus.Skipped);
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
    

    public async Task<DoctorHistoryVM> GetDoctorHistoryAsync(
    DateOnly? dateFrom, DateOnly? dateTo, BookingStatus? status, string? search,
    int page, int pageSize = 20)
    {
        await  _helper.ExpireOldBookingsAsync();
        var doctor = await _doctors.GetAsync();
    
        var baseQuery = _db.Bookings.AsNoTracking().Where(b => b.DoctorId == doctor.Id);
    
        if (dateFrom.HasValue) baseQuery = baseQuery.Where(b => b.Date >= dateFrom.Value);
        if (dateTo.HasValue)   baseQuery = baseQuery.Where(b => b.Date <= dateTo.Value);
    
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var pattern = ArabicSearch.ToLikePattern(s);
        
            baseQuery = baseQuery.Where(b =>
                EF.Functions.Like(b.User.Name, pattern) || b.User.Phone.Contains(s));
        }
    
        // ملخّص الفترة (بدون فلتر الحالة)
        var counts = await baseQuery
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    
        var filtered = status.HasValue ? baseQuery.Where(b => b.Status == status.Value) : baseQuery;
    
        var total = await filtered.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
    
        var items = await filtered
            .Include(b => b.User)
            .OrderByDescending(b => b.Date)
            .ThenBy(b => b.QueueNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
        return new DoctorHistoryVM
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            Status = status,
            Search = search,
            Items = items,
            Counts = counts,
            TotalCount = total,
            Page = page,
            TotalPages = totalPages
        };
    }


}
