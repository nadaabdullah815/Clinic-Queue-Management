using ClinicQueue.Models;

namespace ClinicQueue.ViewModels;

public class BookPageVM
{
    public Doctor Doctor { get; set; } = null!;
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public bool IsFull => BookedCount >= Capacity;
    public int CurrentNumber { get; set; }
    public int WaitingCount { get; set; }
    public bool HasActive { get; set; }
}