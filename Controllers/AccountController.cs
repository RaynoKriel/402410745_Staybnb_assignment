using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using _402410745_Staybnb_assignment.Models;
using _402410745_Staybnb_assignment.Services;
using _402410745_Staybnb_assignment.ViewModels;

namespace _402410745_Staybnb_assignment.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly AuditService _audit;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AuditService audit)
    {
        _users = users;
        _signIn = signIn;
        _audit = audit;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // Every new registrant starts as a Guest.
        await _users.AddToRoleAsync(user, "Guest");
        await _audit.LogAsync(user.Id, "Registered a new account", ActivityType.Registration);
        await _audit.NotifyAsync(user.Id, "Welcome to Staybnb", "Your account is ready. Browse properties or apply to become a Host.", NotificationType.Success);

        await _signIn.SignInAsync(user, false);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var user = await _users.FindByEmailAsync(model.Email);
        await _audit.LogAsync(user.Id, "Logged in", ActivityType.Login);

        // Send each role to its own starting page.
        if (await _users.IsInRoleAsync(user, "SuperAdmin") || await _users.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Dashboard", "Admin");
        if (await _users.IsInRoleAsync(user, "Host"))
            return RedirectToAction("Dashboard", "Host");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
