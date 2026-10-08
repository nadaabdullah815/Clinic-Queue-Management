using ClinicQueue.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicQueue.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Doctors.Any())
        {
            db.Doctors.Add(new Doctor
            {
                Name = "الطبيب محمد خالد العبدالله",
                Specialty = "اختصاصي في طب الفم والأسنان",
                SubSpecialties = "حشوات معدنية وتجميلية|تعويضات متحركة|تيجان وجسور|زيركون|معالجة لبية|قلع|معالجة لثوية|أطفال",
                Location = "جديدة عرطوز,امتدادشارع البلدية شرقا",
                WorkStartTime = new TimeOnly(9, 0),
                WorkEndTime = new TimeOnly(17, 0),
                DailyCapacity = 50,
                IsAcceptingBookings = true
            });
        }

        if (!db.Users.Any(u => u.Role == UserRole.Doctor))
        {
            var doctorUser = new User
            {
                Name = "Dr. Mohammed",
                Phone = "0999999999",
                Role = UserRole.Doctor
            };
            doctorUser.PasswordHash = new PasswordHasher<User>().HashPassword(doctorUser, "Doctor@123");
            db.Users.Add(doctorUser);
        }

        db.SaveChanges();
    }
}