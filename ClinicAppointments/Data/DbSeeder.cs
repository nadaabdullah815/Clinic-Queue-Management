using ClinicAppointments.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicAppointments.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Doctors.Any())
        {
            db.Doctors.Add(new Doctor
            {
                Name = "Dr. Mohammed",
                Specialty = "General Medicine",
                WorkStartTime = new TimeOnly(9, 0),
                WorkEndTime = new TimeOnly(17, 0),
               // DailyCapacity = 50,
                IsAcceptingBookings = true
            });
        }

        if (!db.Users.Any(u => u.Role == UserRole.Doctor))
        {
            var Doctor = new User
            {
                Name = "Clinic Mohammed",
                Phone = "0999999999",
                Role = UserRole.Doctor
            };
            Doctor.PasswordHash = new PasswordHasher<User>().HashPassword(Doctor, "Doctor@123");
            db.Users.Add(Doctor);
        }

        db.SaveChanges();
    }
}