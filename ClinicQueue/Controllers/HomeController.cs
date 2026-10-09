using System.Diagnostics;
using ClinicQueue.Models;
using ClinicQueue.Services;
using Microsoft.AspNetCore.Mvc;
using ClinicQueue.Services.Interfaces;

namespace ClinicQueue.Controllers;

public class HomeController : Controller
{
    private readonly IDoctorProvider _doctors;
    private readonly IPatientQueueService _PattientQueue;
    private readonly IDoctorQueueService _doctorQueue;

    public HomeController(IDoctorProvider doctors, IPatientQueueService patientQueue, IDoctorQueueService doctorQueue)
    {
        _doctors = doctors;
        _PattientQueue = patientQueue;
        _doctorQueue = doctorQueue;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.IsFull = await _doctorQueue.IsFullAsync();
        return View(await _doctors.GetAsync());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}