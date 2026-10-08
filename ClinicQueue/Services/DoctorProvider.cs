using ClinicQueue.Data;
using ClinicQueue.Models;
using Microsoft.EntityFrameworkCore;
using ClinicQueue.Services.Interfaces;

namespace ClinicQueue.Services;
public class DoctorProvider : IDoctorProvider
{
    private readonly AppDbContext _db;
    public DoctorProvider(AppDbContext db) => _db = db;

    // الطبيب الوحيد بالنظام
    public Task<Doctor> GetAsync() => _db.Doctors.AsNoTracking().FirstAsync();
}