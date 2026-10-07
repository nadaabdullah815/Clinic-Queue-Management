using ClinicAppointments.Data;
using ClinicAppointments.Models;
using Microsoft.EntityFrameworkCore;
using ClinicAppointments.Services.Interfaces;

namespace ClinicAppointments.Services;
public class DoctorProvider : IDoctorProvider
{
    private readonly AppDbContext _db;
    public DoctorProvider(AppDbContext db) => _db = db;

    // الطبيب الوحيد بالنظام
    public Task<Doctor> GetAsync() => _db.Doctors.AsNoTracking().FirstAsync();
}