using ClinicQueue.Data;
using ClinicQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueue.Helper;

public interface IPatientHelperFunctions
{
    BookingStatus[] ActiveStatuses { get; }
    Task<int> CountedTodayAsync(int doctorId, DateOnly date);
}

public class PatientHelperFunctions : IPatientHelperFunctions
{
    private readonly AppDbContext _db;
    public BookingStatus[] ActiveStatuses { get; } = { BookingStatus.Waiting, BookingStatus.InProgress };
    private static readonly BookingStatus[] Counted = { BookingStatus.Waiting, BookingStatus.InProgress, BookingStatus.Done };

    public PatientHelperFunctions(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> CountedTodayAsync(int doctorId, DateOnly date) =>
        _db.Bookings.CountAsync(b =>
            b.DoctorId == doctorId && b.Date == date && Counted.Contains(b.Status));
}