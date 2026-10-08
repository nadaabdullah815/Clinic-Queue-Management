using System.ComponentModel.DataAnnotations;

namespace ClinicQueue.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Patient;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}