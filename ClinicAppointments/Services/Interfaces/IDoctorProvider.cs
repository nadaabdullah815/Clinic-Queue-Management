using ClinicAppointments.Data;
using ClinicAppointments.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointments.Services.Interfaces
{
    public interface IDoctorProvider
    {
        Task<Doctor> GetAsync();
    }
}
