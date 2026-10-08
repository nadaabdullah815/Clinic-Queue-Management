using System.Security.Claims;
using ClinicQueue.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicQueue.Controllers;

[Authorize(Roles = "Patient")]
public class BookingController : Controller
{
    private readonly QueueService _queue;

    public BookingController(QueueService queue) => _queue = queue;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Book() => View(await _queue.GetBookPageAsync(UserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm()
    {
        var result = await _queue.BookAsync(UserId);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return result.Success
            ? RedirectToAction(nameof(MyQueue))
            : RedirectToAction(nameof(Book));
    }

    [HttpGet]
    public async Task<IActionResult> MyQueue() => View(await _queue.GetStatusAsync(UserId));

    // يستدعيها الـJavaScript كل 10 ثواني
    [HttpGet]
    public async Task<IActionResult> Status() => Json(await _queue.GetStatusAsync(UserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var ok = await _queue.CancelAsync(id, UserId);
        TempData[ok ? "Success" : "Error"] = ok ? "تم إلغاء حجزك" : "لا يمكن الغاء هذا الحجز";
        return RedirectToAction(nameof(MyQueue));
    }

    [HttpGet]
    public async Task<IActionResult> History() => View(await _queue.GetHistoryAsync(UserId));
}