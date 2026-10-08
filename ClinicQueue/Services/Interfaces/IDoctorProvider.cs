using ClinicQueue.Data;
using ClinicQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicQueue.Services.Interfaces
{
    public interface IDoctorProvider
    {
        Task<Doctor> GetAsync();
    }
}
