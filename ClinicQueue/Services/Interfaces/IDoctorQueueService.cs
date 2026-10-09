using ClinicQueue.Models;
using ClinicQueue.ViewModels;
namespace ClinicQueue.Services.Interfaces;
public interface IDoctorQueueService
{
    Task<List<Booking>> GetTodayAsync();
    Task<bool> NextAsync();
    Task<bool> SkipAsync(int bookingId);
    Task<bool> CancelByDoctorAsync(int bookingId);
    Task<bool> RequeueAsync(int bookingId);
    Task SetAcceptingBookingsAsync(bool open);
    Task UpdateWorkingHoursAsync(TimeOnly start, TimeOnly end);
    Task UpdateCapacityAsync(int capacity);
    Task<bool> IsFullAsync();
    Task<DoctorHistoryVM> GetDoctorHistoryAsync(
    DateOnly? dateFrom, DateOnly? dateTo, BookingStatus? status, string? search,
    int page, int pageSize = 20);
}