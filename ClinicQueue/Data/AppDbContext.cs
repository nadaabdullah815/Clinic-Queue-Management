using ClinicQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueue.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Phone).IsUnique();
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Booking>(e =>
        {

            e.HasIndex(x => new { x.DoctorId, x.Date, x.QueueNumber }).IsUnique();
            e.HasIndex(x => new { x.UserId, x.Date });

            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.User).WithMany(u => u.Bookings)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor).WithMany(d => d.Bookings)
                .HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
        });
        b.Entity<Doctor>()
           .Property(d => d.DailyCapacity)
           .HasDefaultValue(50);
    }
}