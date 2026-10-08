using System.ComponentModel.DataAnnotations;

namespace ClinicQueue.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
    public int DailyCapacity { get; set; } = 50;
    public TimeOnly WorkStartTime { get; set; }
    public TimeOnly WorkEndTime { get; set; }
    [MaxLength(1000)]
    public string? Location { get; set; }
    
    [MaxLength(500)]
    public string? SubSpecialties { get; set; }   // تُفصل بينها العلامة |

   // public int DailyCapacity { get; set; } = 50;
    public bool IsAcceptingBookings { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}