using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClinicAppointments.Models;
using ClinicAppointments.Services.Interfaces;

namespace ClinicAppointments.Controllers;

public class HomeController : Controller
{
    private readonly IDoctorProvider _doctors;
    public HomeController(IDoctorProvider doctors) 
    => _doctors = doctors;

    public async Task<IActionResult> Index() 
    => View(await _doctors.GetAsync());

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
