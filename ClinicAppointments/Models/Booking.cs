namespace ClinicAppointments.Models;

public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }
    public int QueueNumber { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Waiting;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}