using ClinicAppointments.Models;

namespace ClinicAppointments.ViewModels;

public class BookPageVM
{
    public Doctor Doctor { get; set; } = null!;
    public int CurrentNumber { get; set; }
    public int WaitingCount { get; set; }
    public bool HasActive { get; set; }
}