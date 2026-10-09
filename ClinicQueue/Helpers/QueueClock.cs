namespace ClinicQueue.Helpers;

public static class QueueClock
{
    // عند النشر نضبطها على منطقة زمنية محددة (بالتعديل هون فقط)
    public static DateTime Now => DateTime.Now;

    public static DateOnly Today => DateOnly.FromDateTime(Now);
    public static TimeOnly TimeNow => TimeOnly.FromDateTime(Now);
}