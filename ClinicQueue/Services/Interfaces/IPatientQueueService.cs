using ClinicQueue.Models;
using ClinicQueue.ViewModels; 

namespace ClinicQueue.Services;
public interface IPatientQueueService
{
    Task<BookResult> BookAsync(int userId);
    Task<bool> CancelAsync(int bookingId, int userId);
    Task<QueueStatusVM> GetStatusAsync(int userId);
    Task<BookPageVM> GetBookPageAsync(int userId);
    Task<List<Booking>> GetHistoryAsync(int userId);
}