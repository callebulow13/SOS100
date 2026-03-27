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
        // Uppdatera watch-status från KatalogApi
        try
        {
            var katalogClient = _httpClientFactory.CreateClient();
            var katalogBaseUrl = _configuration["KatalogApiBaseUrl"] ?? "http://localhost:5000";
            foreach (var watch in watches)
            {
                var itemResponse = await katalogClient.GetAsync($"{katalogBaseUrl}/api/items/{watch.ItemId}");
                if (itemResponse.IsSuccessStatusCode)
                {
                    var item = await itemResponse.Content.ReadFromJsonAsync<ItemDto>();
                    if (item != null)
                        watch.IsActive = item.Status == 0; // 0 = Tillgänglig
                }
            }
        }
        catch
        {
            // KatalogApi är nere - visa befintlig status
        }
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
            
            var loanApiKey = _configuration["LoanApiKey"]; 
            if (!string.IsNullOrEmpty(loanApiKey))
            {
                loanClient.DefaultRequestHeaders.Add("X-Api-Key", loanApiKey);
            }
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
    [HttpPost]
    public async Task<IActionResult> StopWatch(int watchId)
    {
        try
        {
            // Vi använder din tjänst som redan har API-nyckeln konfigurerad!
            await _reminderService.DeleteWatchAsync(watchId);
        
            // Vi sparar ett meddelande om att det lyckades
            TempData["SuccessMessage"] = "Bevakningen har avslutats.";
        }
        catch (Exception ex)
        {
            // Fånga felet och spara det så att vi faktiskt kan se vad som gick fel
            TempData["ErrorMessage"] = $"Kunde inte avsluta bevakningen: {ex.Message}";
            Console.WriteLine($"⚠️ Fel vid StopWatch: {ex.Message}");
        }
    
        return RedirectToAction("Index");
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
        [HttpPost]
    public async Task<IActionResult> ReturnItem(Guid loanId, int userId)
    {
        var client = _httpClientFactory.CreateClient();
        var loanBaseUrl = _configuration["LoanApiBaseUrl"] ?? "http://localhost:5125";
        var loanApiKey = _configuration["LoanApiKey"];
        if (!string.IsNullOrEmpty(loanApiKey))
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", loanApiKey);
        }

        // ── Steg 1: Återlämna lånet ──
        var response = await client.PostAsync(
            $"{loanBaseUrl}/api/loans/{loanId}/return", null);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Kunde inte återlämna. API svarade: {errorText}";
            return RedirectToAction("Index", new { id = userId });
        }

        // ── Steg 2: Hitta reminder för detta lån och markera som skickad ──
        try
        {
            var reminderBaseUrl = _configuration["ReminderServiceBaseUrl"] ?? "http://localhost:5038";
            var reminderApiKey = _configuration["ReminderApiKey"] ?? "reminder-hemlig-123";

            var reminderClient = _httpClientFactory.CreateClient();
            reminderClient.DefaultRequestHeaders.Add("X-Api-Key", reminderApiKey);

            // Hämta alla reminders
            var remindersResponse = await reminderClient.GetAsync(
                $"{reminderBaseUrl}/api/reminders");

            if (remindersResponse.IsSuccessStatusCode)
            {
                var reminders = await remindersResponse.Content
                    .ReadFromJsonAsync<List<ReminderDto>>();

                // Hitta reminder som matchar detta lån (loanId som sträng)
                var match = reminders?.FirstOrDefault(r =>
                    r.LoanId == loanId.ToString() ||
                    r.UserId == userId.ToString());

                if (match != null)
                {
                    // Markera som skickad
                    var updated = new
                    {
                        isSent = true,
                        dueDate = match.DueDate,
                        itemTitle = match.ItemTitle
                    };

                    await reminderClient.PutAsJsonAsync(
                        $"{reminderBaseUrl}/api/reminders/{match.Id}", updated);

                    Console.WriteLine($"✅ Reminder {match.Id} markerad som skickad!");
                }
            }
        }
        catch (Exception ex)
        {
            // Logga men avbryt inte — lånet är redan återlämnat
            Console.WriteLine($"⚠️ Kunde inte uppdatera reminder: {ex.Message}");
        }

        return RedirectToAction("Index", new { id = userId });
    }
}