using ClinicQueue.Models;
using ClinicQueue.Services;
using ClinicQueue.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicQueue.Services.Interfaces;
using ClinicQueue.Helpers;
namespace ClinicQueue.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorQueueController : Controller
{
    private readonly IDoctorQueueService _queue;
    private readonly IDoctorProvider _doctors;

    public DoctorQueueController(IDoctorQueueService queueService, IDoctorProvider doctors)
    {
        _queue = queueService;
        _doctors = doctors;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var doctor = await _doctors.GetAsync();
        var list = await _queue.GetTodayAsync();

        var vm = new DoctorDashboardVM
        {
            DoctorName = doctor.Name,
            Date = QueueClock.Today,
            WorkStartTime = doctor.WorkStartTime,
            WorkEndTime = doctor.WorkEndTime,
            DailyCapacity = doctor.DailyCapacity,
            BookedCount = list.Count(b => b.Status == BookingStatus.Waiting
                           || b.Status == BookingStatus.InProgress
                           || b.Status == BookingStatus.Done),
            IsAcceptingBookings = doctor.IsAcceptingBookings,
            Current = list.FirstOrDefault(b => b.Status == BookingStatus.InProgress),
            Next = list.Where(b => b.Status == BookingStatus.Waiting)
                       .OrderBy(b => b.QueueNumber).FirstOrDefault(),
            WaitingCount = list.Count(b => b.Status == BookingStatus.Waiting),
            DoneCount = list.Count(b => b.Status == BookingStatus.Done),
            Bookings = list
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Next()
    {
        var hasNext = await _queue.NextAsync();
        if (hasNext) TempData["Success"] = "تم استدعاء المريض التالي";
        else TempData["Error"] = "انتهت قائمة اليوم، لا يوجد مرضى بالانتظار";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Skip(int id)
    {
        var ok = await _queue.SkipAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "تم تخطي الدور" : "تعذّر تنفيذ العملية";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _queue.CancelByDoctorAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "تم إلغاء الدور" : "تعذّر تنفيذ العملية";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Requeue(int id)
    {
        var ok = await _queue.RequeueAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "أُعيد المريض إلى قائمة الانتظار" : "تعذّر تنفيذ العملية";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBooking(bool open)
    {
        await _queue.SetAcceptingBookingsAsync(open);
        TempData["Success"] = open ? "تم فتح باب الحجز" : "تم إيقاف استقبال الحجوزات";
        return RedirectToAction(nameof(Dashboard));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateHours(TimeOnly start, TimeOnly end)
    {
        if (end <= start)
        {
            TempData["Error"] = "يجب أن يكون وقت انتهاء الدوام بعد وقت بدايته";
            return RedirectToAction(nameof(Dashboard));
        }
    
        await _queue.UpdateWorkingHoursAsync(start, end);
        TempData["Success"] = "تم تحديث ساعات الدوام";
        return RedirectToAction(nameof(Dashboard));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCapacity(int capacity)
    {
        if (capacity < 1 || capacity > 500)
        {
            TempData["Error"] = "يجب أن يكون الحد الأقصى بين 1 و500";
            return RedirectToAction(nameof(Dashboard));
        }
    
        await _queue.UpdateCapacityAsync(capacity);
        TempData["Success"] = "تم تحديث الحد الأقصى للحجوزات";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> History(
        DateOnly? dateFrom, DateOnly? dateTo, BookingStatus? status, string? search, int page = 1)
    {
        // إذا كانت الفترة معكوسة نبدّل بين التاريخين بدل ما نرفض
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            (dateFrom, dateTo) = (dateTo, dateFrom);
    
        var vm = await _queue.GetDoctorHistoryAsync(dateFrom, dateTo, status, search, page);
        return View(vm);
    }
}