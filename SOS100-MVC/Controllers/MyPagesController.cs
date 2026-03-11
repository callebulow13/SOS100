using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SOS100_MVC.Models;

namespace SOS100_MVC.Controllers;

public class MyPagesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ReminderServiceClient _reminderService;
    
    public MyPagesController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ReminderServiceClient reminderService)
    {
        _reminderService = reminderService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out int userIdInt))
            return Content("Invalid user id");

        // Hämta påminnelser och bevakningar från ReminderService
        var reminders = await _reminderService.GetRemindersAsync(userIdInt);
        var watches = await _reminderService.GetWatchesAsync(userIdInt);
        var overdueCount = await _reminderService.GetOverdueCountAsync();

        ViewBag.Reminders = reminders;
        ViewBag.Watches = watches;
        ViewBag.OverdueCount = overdueCount;

        // Hämta användare från UserService
        User? user = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["UserServiceBaseUrl"];
            var response = await client.GetAsync($"{baseUrl}/User/{userId}");
            if (response.IsSuccessStatusCode)
                user = await response.Content.ReadFromJsonAsync<User>();
        }
        catch
        {
            // UserService är nere
        }

        // Hämta aktiva lån från LoanService
        var activeLoans = new List<LoanDto>();
        try
        {
            var loanClient = _httpClientFactory.CreateClient();
            var loanBaseUrl = _configuration["LoanApiBaseUrl"] ?? "http://localhost:5125";
            var loanResponse = await loanClient.GetAsync($"{loanBaseUrl}/api/loans");
            
            if (loanResponse.IsSuccessStatusCode)
            {
                var allLoans = await loanResponse.Content
                    .ReadFromJsonAsync<List<LoanDto>>();
                if (allLoans != null)
                {
                    activeLoans = allLoans
                        .Where(l => l.ReturnedAt == null && 
                               l.BorrowerId == userId)
                        .ToList();
                }
            }
        }
        catch
        {
            // LoanService är nere
        }

        ViewBag.ActiveLoans = activeLoans;

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        User? user = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["UserServiceBaseUrl"];
            var response = await client.GetAsync($"{baseUrl}/User/{userId}");
            if (response.IsSuccessStatusCode)
                user = await response.Content.ReadFromJsonAsync<User>();
        }
        catch
        {
            return Content("Kan inte nå UserService");
        }

        return View(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> EditProfile(User user)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["UserServiceBaseUrl"];
            var response = await client.PutAsJsonAsync($"{baseUrl}/User/profile/{user.UserID}", user);
            if (!response.IsSuccessStatusCode)
                return Content("Update failed");
        }
        catch
        {
            return Content("Kan inte nå UserService");
        }

        return RedirectToAction("Index");
    }
}