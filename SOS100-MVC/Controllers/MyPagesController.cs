using Microsoft.AspNetCore.Mvc;
using SOS100_MVC.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SOS100_MVC.Dtos;
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
        // Hämta strängen direkt (t.ex. "1")
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
        if (string.IsNullOrEmpty(userId))
            return Content("Invalid user id");

        // Skicka in strängen utan att göra om den till en int först
        var reminders = await _reminderService.GetRemindersAsync(userId);
        var watches = await _reminderService.GetWatchesAsync(userId);
        var overdueCount = await _reminderService.GetOverdueCountAsync();

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
                                    (l.BorrowerId == userId || 
                                     l.BorrowerId == user?.Username))
                        .ToList();
                }
            }
        }
        catch
        {
            // LoanService är nere
        }

        ViewBag.ActiveLoans = activeLoans;
        ViewBag.Reminders = reminders;
        ViewBag.Watches = watches;

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

        var viewmodel = new EditProfileViewModel
        {
            User = user,
            PasswordDto = new PasswordDto()
        };

        return View(viewmodel);
    }
    
    [HttpPost]
    public async Task<IActionResult> EditProfile(User user)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["UserServiceBaseUrl"];
            var response = await client.PutAsJsonAsync($"{baseUrl}/User/profile/{userId}", user);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
            return Content($"Error från API: {error}");
                
            }
        }
        
        catch
        {
            return Content("Kan inte nå UserService");
        }

        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> EditPassword(EditProfileViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["UserServiceBaseUrl"];
            var response = await client.PutAsJsonAsync($"{baseUrl}/User/changePassword/{userId}", model.PasswordDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Content($"Error från API: {error}");
                }
        }
        catch
        {
            return Content("Kan inte nå UserService");
        }

        return RedirectToAction("Index");
    }
}