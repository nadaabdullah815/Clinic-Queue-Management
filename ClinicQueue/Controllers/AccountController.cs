using System.Security.Claims;
using ClinicQueue.Data;
using ClinicQueue.Models;
using ClinicQueue.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions; 

namespace ClinicQueue.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public AccountController(AppDbContext db) => _db = db;

    // ---------- Register ----------
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var phone = vm.Phone.Trim();
        if (await _db.Users.AnyAsync(u => u.Phone == phone))
        {
            ModelState.AddModelError(nameof(vm.Phone), "رقم الهاتف مسجّل مسبقاً");
            return View(vm);
        }

        var user = new User
        {
            Name = Regex.Replace(vm.Name.Trim(), @"\s+", " "),
            Phone = phone,
            Role = UserRole.Patient // أي حساب جديد = مريض دائماً
        };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInAsync(user);
        return RedirectToAction("Index", "Home");
    }

    // ---------- Login ----------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var phone = vm.Phone.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);

        var ok = user != null &&
                 _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password)
                     != PasswordVerificationResult.Failed;

        if (!ok)
        {
            // رسالة واحدة لا تكشف أي الاثنين غلط
            ModelState.AddModelError(string.Empty, "رقم الهاتف أو كلمة المرور غير صحيحة");
            return View(vm);
        }

        await SignInAsync(user!);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // TODO: لما نبني DoctorQueue نحوّل الطبيب لـ Dashboard
      return user!.Role == UserRole.Doctor
    ? RedirectToAction("Dashboard", "DoctorQueue")
    : RedirectToAction("Index", "Home");
    }

    // ---------- Logout ----------
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    // ---------- helper ----------
    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()) // "Patient" أو "Doctor"
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}