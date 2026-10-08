using System.Diagnostics;
using ClinicQueue.Models;
using ClinicQueue.Services;
using Microsoft.AspNetCore.Mvc;
using ClinicQueue.Services.Interfaces;

namespace ClinicQueue.Controllers;

public class HomeController : Controller
{
    private readonly IDoctorProvider _doctors;
    private readonly QueueService _queue;

    public HomeController(IDoctorProvider doctors, QueueService queue)
    {
        _doctors = doctors;
        _queue = queue;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.IsFull = await _queue.IsFullAsync();
        return View(await _doctors.GetAsync());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}